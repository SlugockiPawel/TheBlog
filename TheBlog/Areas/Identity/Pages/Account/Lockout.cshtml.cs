using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TheBlog.Areas.Identity.Pages.Account;

[AllowAnonymous]
public sealed class LockoutModel : PageModel
{
    public void OnGet()
    {
    }
}