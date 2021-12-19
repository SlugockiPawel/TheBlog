using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.Build.Tasks;
using Microsoft.EntityFrameworkCore;
using TheBlog.Data;
using TheBlog.Enums;
using TheBlog.Models;
using TheBlog.Services;
using X.PagedList;

namespace TheBlog.Areas.Identity.Pages.Account.Manage
{

    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrator")]
    public class ManageRolesModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BlogUser> _userManager;

        public ManageRolesModel(ApplicationDbContext context, UserManager<BlogUser> userManager)
        {
            _context = context;
            _userManager = userManager;
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
            await PopulateInputModel();

            return Page();
        }

        public async Task<IActionResult> OnPostAddRoleToUserAsync(string userId, string futureRole, [CanBeNull] string command)
        {
            if (command is not null)
            {
                futureRole = string.Equals(command, "Admin") ? BlogRole.Administrator.ToString() : BlogRole.Moderator.ToString();
            }
            
            var user = await _userManager.FindByIdAsync(userId);

            if (user is null)
            {
                ModelState.AddModelError("ModelError", "User is not in database, try again");
            }

            if ( await _userManager.IsInRoleAsync(user, futureRole))
            {
                ModelState.AddModelError("ModelError", $"{user} is already in {futureRole} role");
            }

            if (!ModelState.IsValid)
            {
                await PopulateInputModel();

                return Page();
            }

            await _userManager.AddToRoleAsync(user, futureRole);
            await _userManager.UpdateAsync(user);

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRemoveRoleFromUserAsync(string userId, string deleteRole)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user is null)
            {
                ModelState.AddModelError("ModelError", "User is not in database, try again");
            }

            if (user.Email == "slugocki.pawel@gmail.com" && deleteRole == BlogRole.Administrator.ToString())
            {
                ModelState.AddModelError("ModelError", "Cannot revoke Administrator role from app owner");
            }

            if (!await _userManager.IsInRoleAsync(user, deleteRole))
            {
                ModelState.AddModelError("ModelError", "User is not in this role, delete unsuccessful");
            }

            if (!ModelState.IsValid)
            {
                await PopulateInputModel();

                return Page();
            }

            await _userManager.RemoveFromRoleAsync(user, deleteRole);
            await _userManager.UpdateAsync(user);

            return RedirectToPage();
        }

        //used framework GetUsersInRoleAsync() instead of below custom one

        // private async Task<List<BlogUser>> GetUsersByRoleAsync(string blogUserRole)
        // {
        //     if (blogUserRole is null ||
        //         !Enum.IsDefined(typeof(BlogRole), blogUserRole))
        //     {
        //         return null;
        //     }
        //
        //     return await (from user in _context.Users
        //         join userRole in _context.UserRoles on user.Id equals userRole.UserId
        //         join role in _context.Roles on userRole.RoleId equals role.Id
        //         where role.Name == blogUserRole
        //         select user).ToListAsync();
        // }

        private async Task<List<BlogUser>> GetUsersWithoutRoleAsync()
        {
            var roles = Enum.GetNames(typeof(BlogRole));

            return await (from user in _context.Users
                    join userRole in _context.UserRoles on user.Id equals userRole.UserId
                        into userUserRoleGroup
                    from userRole in
                        userUserRoleGroup
                            .DefaultIfEmpty() //DefaultIfEmpty() performs LEFT JOIN so it will return all users, not only with Roles
                    join role in _context.Roles on userRole.RoleId equals role.Id
                        into roleGroup
                    from role in roleGroup.DefaultIfEmpty()
                    select new
                    {
                        user,
                        role,
                    }
                ).Where(x => roles.All(x2 => x2 != x.role.Name)).Select(x => x.user) // get all users without any role
                .ToListAsync();
        }

        private async Task PopulateInputModel()
        {
            Input.Admins = await _userManager.GetUsersInRoleAsync(BlogRole.Administrator.ToString()).Result.ToListAsync();
            Input.Moderators = await _userManager.GetUsersInRoleAsync(BlogRole.Moderator.ToString()).Result.ToListAsync();
            Input.NormalUsers = await GetUsersWithoutRoleAsync();

        }
    }
}