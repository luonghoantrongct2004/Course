using EduCourse.Areas.Admin.Models;
using EduCourse.Data;
using EduCourse.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EduCourse.Areas.Admin.Controllers;
[Area("Admin")]
[Authorize]
public class StudentController : Controller
{
    private readonly AppDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public StudentController(AppDbContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
    {
        // Get the total count of students in the "Student" role
        var totalStudents = (await _userManager.GetUsersInRoleAsync("Student")).Count;

        // Fetch the paginated students
        var students = (await _userManager.GetUsersInRoleAsync("Student"))
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // Add UserCourses and StudentExams to ViewData
        ViewData["UserCourse"] = _context.UserCourses.ToList();
        ViewData["Exam"] = _context.StudentExams.ToList();

        // Set ViewData for pagination
        ViewData["TotalStudents"] = totalStudents;
        ViewData["CurrentPage"] = page;
        ViewData["PageSize"] = pageSize;

        return View(students);
    }

    public IActionResult Details(string id)
    {
        var user = _context.Users.Find(id);
        if (user == null) return Redirect("/Home/Error404");
        Guid.TryParse(user.Id, out var userId);
        ViewData["Order"] = _context.Orders.Where(u => u.UserID == id).Count();
        ViewData["Cart"] = _context.Carts.Where(u => u.UserId == userId).Count();
        ViewData["Exam"] = _context.StudentExams.Where(u => u.StudentID == id).Count();
        return View(user);
    }
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }
    [HttpPost]
    public async Task<IActionResult> Create(StudentCreateViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new User
            {
                UserName = model.Email,
                FullName = model.FullName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                DateJoined = DateTime.Now,
                Status = model.Status,
                Address = model.Address,
            };

            // Kiểm tra và xử lý ảnh (photo) nếu được tải lên
            if (model.Photo != null && model.Photo.Length > 0)
            {
                var fileName = Path.GetFileName(model.Photo.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.Photo.CopyToAsync(stream);
                }

                // Gán đường dẫn ảnh vào thuộc tính Image của User
                user.Image = "/uploads/" + fileName;
            }

            // Tạo người dùng mới bằng UserManager
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                if (!await _roleManager.RoleExistsAsync("Student"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Student"));
                }

                await _userManager.AddToRoleAsync(user, "Student");

                return Json(new { success = true, message = "Tạo tài khoản thành công!" });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
                return Json(new { success = false, message = error.Description });
            }
        }

        return Json(new { success = false, message = "Có lỗi xảy ra !" });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return Redirect("/Home/Error404");
        }

        return View(user);
    }
    [HttpPost]
    public async Task<IActionResult> Edit(string id, string fullName, string email, string phoneNumber, string address, string status, IFormFile photo)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user != null)
        {
            user.FullName = fullName;
            user.Email = email;
            user.PhoneNumber = phoneNumber;
            user.Address = address;
            user.Status = status;

            // Nếu có ảnh mới, lưu ảnh và cập nhật thông tin
            if (photo != null && photo.Length > 0)
            {
                var fileName = Path.GetFileName(photo.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await photo.CopyToAsync(stream);
                }

                user.Image = "/uploads/" + fileName;
            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return Json(new { success = true, message = "Cập nhật thông tin thành công!" });
            }

            return Json(new { success = false, message = "Đã có lỗi xảy ra khi cập nhật thông tin." });
        }

        return Json(new { success = false, message = "Không tìm thấy học sinh." });
    }


    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            return Redirect("/Home/Error404");
        }

        var result = await _userManager.DeleteAsync(user);

        if (result.Succeeded)
        {
            return RedirectToAction("Index");
        }

        ModelState.AddModelError(string.Empty, "Xóa người dùng không thành công.");
        return RedirectToAction("Index");
    }

}
