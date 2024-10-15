using EduCourse.Areas.Admin.Models;
using EduCourse.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EduCourse.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class RoleController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public RoleController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<IActionResult> Index()
    {
        // Get all users
        var users = _userManager.Users.ToList();

        // Get all roles
        var roles = _roleManager.Roles.ToList();

        // Create a dictionary to store the roles for each user
        var userRoles = new Dictionary<string, List<string>>();

        // Populate the dictionary with users and their roles
        foreach (var user in users)
        {
            var rolesForUser = await _userManager.GetRolesAsync(user); // Get roles for each user
            userRoles[user.Id] = rolesForUser.ToList();
        }

        // Create the view model
        var viewModel = new UserRoleViewModel
        {
            Users = users,
            Roles = roles,
            UserRoles = userRoles
        };

        return View(viewModel);
    }
    [HttpPost]
    public async Task<IActionResult> ChangeUserRole(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        // Remove all roles from the user
        var userRoles = await _userManager.GetRolesAsync(user);
        var result = await _userManager.RemoveFromRolesAsync(user, userRoles);

        if (!result.Succeeded)
        {
            // Handle error
            return BadRequest("Failed to remove user roles.");
        }

        // Add the selected role to the user
        result = await _userManager.AddToRoleAsync(user, role);
        if (!result.Succeeded)
        {
            // Handle error
            return BadRequest("Failed to assign the new role.");
        }

        return Redirect("/admin/role");
    }

    public async Task<IActionResult> AssignRole(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Redirect("/Home/Error404");
        }

        if (!await _roleManager.RoleExistsAsync(role))
        {
            return BadRequest("Role does not exist");
        }

        var result = await _userManager.AddToRoleAsync(user, role);
        if (result.Succeeded)
        {
            return Ok("Role assigned successfully");
        }

        return BadRequest("Failed to assign role");
    }

}
