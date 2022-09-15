using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TheBlog.Areas.Identity.Pages.Account.Manage;

public sealed class ShowRecoveryCodesModel : PageModel
{
    [TempData] public string[] RecoveryCodes { get; set; }

    [TempData] public string StatusMessage { get; set; }

    public IActionResult OnGet()
    {
        if (RecoveryCodes == null || RecoveryCodes.Length == 0) return RedirectToPage("./TwoFactorAuthentication");

        return Page();
    }
}