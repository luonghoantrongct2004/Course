using EduCourse.Areas.Admin.Models;
using EduCourse.Data;
using EduCourse.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduCourse.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class RoleController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly AppDbContext _context;

    public RoleController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, AppDbContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
    {
        // Get all users
        var users = _context.Users
            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
            .ToList();

        // Get all roles
        var roles = _roleManager.Roles
             .Skip((page - 1) * pageSize)
                            .Take(pageSize)
            .ToList();

        // Create a dictionary to store the roles for each user
        var userRoles = new Dictionary<string, List<string>>();

        // Populate the dictionary with users and their roles
        foreach (var user in users)
        {
            var rolesForUser = await _userManager.GetRolesAsync(user); // Get roles for each user
            userRoles[user.Id] = rolesForUser.ToList();
        }

        var totalOrders = users.Count();

        ViewData["TotalUser"] = totalOrders;
        ViewData["CurrentPage"] = page;
        ViewData["PageSize"] = pageSize;
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
        // Tìm người dùng theo Id
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        // Lấy danh sách các vai trò hiện tại của người dùng
        var userRoles = await _userManager.GetRolesAsync(user);

        // Kiểm tra xem người dùng đã có vai trò mong muốn chưa
        if (userRoles.Contains(role))
        {
            // Nếu người dùng đã có vai trò này, không cần thay đổi
            return BadRequest("User already has this role.");
        }

        // Xóa tất cả các vai trò cũ của người dùng
        var result = await _userManager.RemoveFromRolesAsync(user, userRoles);

        if (!result.Succeeded)
        {
            // Xử lý lỗi nếu việc xóa vai trò thất bại
            return BadRequest("Failed to remove user roles.");
        }

        // Thêm vai trò mới cho người dùng
        result = await _userManager.AddToRoleAsync(user, role);
        if (!result.Succeeded)
        {
            // Xử lý lỗi nếu việc thêm vai trò thất bại
            return BadRequest("Failed to assign the new role.");
        }

        // Chuyển hướng lại trang quản lý vai trò
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
