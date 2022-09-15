using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TheBlog.Models;
using TheBlog.Services;

namespace TheBlog.Areas.Identity.Pages.Account;

[AllowAnonymous]
public sealed class ExternalLoginModel : PageModel
{
    private readonly IConfiguration _configuration;
    private readonly IBlogEmailSender _emailSender;
    private readonly IImageService _imageService;
    private readonly ILogger<ExternalLoginModel> _logger;
    private readonly SignInManager<BlogUser> _signInManager;
    private readonly UserManager<BlogUser> _userManager;

    public ExternalLoginModel(
        SignInManager<BlogUser> signInManager,
        UserManager<BlogUser> userManager,
        ILogger<ExternalLoginModel> logger,
        IBlogEmailSender emailSender,
        IConfiguration configuration,
        IImageService imageService
    )
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
        _emailSender = emailSender;
        _configuration = configuration;
        _imageService = imageService;
    }

    [BindProperty] public InputModel Input { get; set; }

    public string ProviderDisplayName { get; set; }

    public string ReturnUrl { get; set; }

    [TempData] public string ErrorMessage { get; set; }

    public IActionResult OnGetAsync()
    {
        return RedirectToPage("./Login");
    }

    public IActionResult OnPost(string provider, string returnUrl = null)
    {
        // Request a redirect to the external login provider.
        var redirectUrl = Url.Page("./ExternalLogin", "Callback", new { returnUrl });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(
            provider,
            redirectUrl
        );
        return new ChallengeResult(provider, properties);
    }

    public async Task<IActionResult> OnGetCallbackAsync(
        string returnUrl = null,
        string remoteError = null
    )
    {
        returnUrl = returnUrl ?? Url.Content("~/");
        if (remoteError != null)
        {
            ErrorMessage = $"Error from external provider: {remoteError}";
            return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
        }

        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            ErrorMessage = "Error loading external login information.";
            return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
        }

        // Sign in the user with this external login provider if the user already has a login.
        var result = await _signInManager.ExternalLoginSignInAsync(
            info.LoginProvider,
            info.ProviderKey,
            false,
            true
        );
        if (result.Succeeded)
        {
            _logger.LogInformation(
                "{Name} logged in with {LoginProvider} provider",
                info.Principal.Identity.Name,
                info.LoginProvider
            );
            return LocalRedirect(returnUrl);
        }

        if (result.IsLockedOut)
            return RedirectToPage("./Lockout");

        // If the user does not have an account, then ask the user to create an account.
        ReturnUrl = returnUrl;
        ProviderDisplayName = info.ProviderDisplayName;
        if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
            Input = new InputModel { Email = info.Principal.FindFirstValue(ClaimTypes.Email) };

        return Page();
    }

    public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null)
    {
        returnUrl = returnUrl ?? Url.Content("~/");
        // Get the information about the user from the external login provider
        var info = await _signInManager.GetExternalLoginInfoAsync();

        if (info == null)
        {
            ErrorMessage = "Error loading external login information during confirmation.";
            return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
        }

        if (ModelState.IsValid)
        {
            var names = info.Principal.Identity?.Name?.Split(' ');

            BlogUser user =
                new()
                {
                    ImageData = await _imageService.EncodeImageAsync(
                        _configuration["DefaultUserImage"]
                    ),
                    ContentType = Path.GetExtension(_configuration["DefaultUserImage"])
                };

            if (names is not null && names.Length > 1)
            {
                user.FirstName = names[0];
                user.LastName = names[1];
                user.Email = Input.Email;
            }
            else
            {
                user.FirstName = names[0];
                user.LastName = names[0];
                user.Email = Input.Email;
            }

            user.UserName = user.Email;
            user.EmailConfirmed = true;

            var result = await _userManager.CreateAsync(user);
            if (result.Succeeded)
            {
                result = await _userManager.AddLoginAsync(user, info);
                if (result.Succeeded)
                {
                    _logger.LogInformation(
                        "User created an account using {Name} provider",
                        info.LoginProvider
                    );

                    var userId = await _userManager.GetUserIdAsync(user);

                    if (userId is not null)
                    {
                        await _signInManager.SignInAsync(user, false, info.LoginProvider);
                        return LocalRedirect(returnUrl);
                    }

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        null,
                        new { area = "Identity", userId, code },
                        Request.Scheme
                    );

                    await _emailSender.SendEmailAsync(
                        Input.Email,
                        "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>."
                    );

                    // If account confirmation is required, we need to show the link if we don't have a real email sender
                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                        return RedirectToPage("./RegisterConfirmation", new { Input.Email });

                    await _signInManager.SignInAsync(user, false, info.LoginProvider);

                    return LocalRedirect(returnUrl);
                }
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
        }

        ProviderDisplayName = info.ProviderDisplayName;
        ReturnUrl = returnUrl;
        return Page();
    }

    public sealed class InputModel
    {
        [Required] [EmailAddress] public string Email { get; set; }
    }
}