using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TheBlog.Data;
using TheBlog.Enums;
using TheBlog.Models;

namespace TheBlog.Areas.Identity.Pages.Account.Manage
{
    [Authorize(Roles = "Administrator")]
    public class ManageRolesModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ManageRolesModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty] public InputModel Input { get; set; } = new();

        public class InputModel
        {
            public List<BlogUser> Admins { get; set; } = new();
            public List<BlogUser> Moderators { get; set; } = new();
            public List<BlogUser> NormalUsers { get; set; } = new();
        }



        public async Task<IActionResult> OnGetAsync()
        {
          

            Input.Admins = await GetUsersByRoleAsync(BlogRole.Administrator.ToString());

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }


            return RedirectToPage();
        }

        private async Task<List<BlogUser>> GetUsersByRoleAsync(string blogUserRole)
        {
            if (blogUserRole is null ||
                !Enum.IsDefined(typeof(BlogRole), blogUserRole))
            {
                return null;
            }

            return await (from user in _context.Users
                join userRole in _context.UserRoles on user.Id equals userRole.UserId
                join role in _context.Roles on userRole.RoleId equals role.Id
                where role.Name == blogUserRole
                select user).ToListAsync();
        }
    }
}