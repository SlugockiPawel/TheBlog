using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TheBlog.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public sealed class ResetPasswordConfirmationModel : PageModel
    {
        public void OnGet()
        {

        }
    }
}
