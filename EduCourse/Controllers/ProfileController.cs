using EduCourse.Data;
using EduCourse.Entities;
using EduCourse.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Drawing.Printing;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace EduCourse.Controllers;

public class ProfileController : Controller
{
    private readonly AppDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;

    public ProfileController(AppDbContext context, UserManager<User> userManager, SignInManager<User> signInManager)
    {
        _context = context;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public IActionResult Index(int page = 1, int pageSize = 10)
    {
        if (!User.Identity.IsAuthenticated)
        {
            return Redirect("/Auth/Login");
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var countCourseEnrolled = _context.Orders.Where(o => o.UserID == userId).Count();
        var examCount = _context.StudentExams.Where(u => u.StudentID == userId).Count();

        var totalUserCourses = _context.UserCourses
            .Where(uc => uc.UserID == userId)
            .Count();

        var userCourses = _context.UserCourses
            .Where(uc => uc.UserID == userId)
            .Include(u => u.User)
            .Include(uc => uc.Course)
                .ThenInclude(c => c.Chapters)
                    .ThenInclude(l => l.Lessons)
            .Skip((page - 1) * pageSize) // Bỏ qua số khóa học đã hiển thị
            .Take(pageSize) // Lấy số khóa học theo pageSize
            .ToList();

        var viewModel = new ProfileViewModel
        {
            EnrollCourse = countCourseEnrolled,
            Exam = examCount,
            User = _context.Users.FirstOrDefault(u => u.Id == userId),
            UserCourses = userCourses,
            CurrentPage = page,
            PageSize = pageSize,
            TotalOrders = countCourseEnrolled,
            TotalPages = (int)Math.Ceiling((double)totalUserCourses / pageSize) // Tính tổng số trang
        };

        return View(viewModel);
    }
    public IActionResult CourseProfile(int page = 1, int pageSize = 10)
    {
        if (!User.Identity.IsAuthenticated)
        {
            return Redirect("/Auth/Login");
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var countCourseEnrolled = _context.Orders.Where(o => o.UserID == userId).Count();
        var examCount = _context.StudentExams.Where(u => u.StudentID == userId).Count();

        var totalUserCourses = _context.UserCourses
            .Where(uc => uc.UserID == userId)
            .Count();

        var userCourses = _context.UserCourses
            .Where(uc => uc.UserID == userId)
            .Include(u => u.User)
            .Include(uc => uc.Course)
                .ThenInclude(c => c.Chapters)
                    .ThenInclude(l => l.Lessons)
            .Skip((page - 1) * pageSize) // Bỏ qua số khóa học đã hiển thị
            .Take(pageSize) // Lấy số khóa học theo pageSize
            .ToList();

        var viewModel = new ProfileViewModel
        {
            EnrollCourse = countCourseEnrolled,
            Exam = examCount,
            User = _context.Users.FirstOrDefault(u => u.Id == userId),
            UserCourses = userCourses,
            CurrentPage = page,
            PageSize = pageSize,
            TotalOrders = countCourseEnrolled,
            TotalPages = (int)Math.Ceiling((double)totalUserCourses / pageSize) // Tính tổng số trang
        };

        return View(viewModel);
    }

    public IActionResult HistoryOrder(string timeFrame = "today", int page = 1, int pageSize = 10)
    {
        if (!User.Identity.IsAuthenticated)
        {
            return Redirect("/Auth/Login");
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var query = _context.Orders.Include(d => d.OrderDetails)
                                     .ThenInclude(c => c.Course)
                                     .Where(u => u.UserID == userId);

        // Filter based on the time frame
        if (timeFrame == "today")
        {
            query = query.Where(o => o.OrderDate.Date == DateTime.Today);
        }
        else if (timeFrame == "month")
        {
            query = query.Where(o => o.OrderDate.Month == DateTime.Now.Month && o.OrderDate.Year == DateTime.Now.Year);
        }
        else if (timeFrame == "year")
        {
            query = query.Where(o => o.OrderDate.Year == DateTime.Now.Year);
        }

        var totalOrders = query.ToList();
        var orders = totalOrders.Skip((page - 1) * pageSize)
                                .Take(pageSize)
                                .ToList();

        var viewModel = new ProfileViewModel
        {
            User = _context.Users.FirstOrDefault(u => u.Id == userId),
            Orders = orders,
            TotalOrders = totalOrders.Count,
            CurrentPage = page,
            PageSize = pageSize,
            TimeFrame = timeFrame // Set the TimeFrame
        };

        return View(viewModel);
    }
    public async Task<IActionResult> Account()
    {
        if (!User.Identity.IsAuthenticated)
        {
            return Redirect("/Auth/Login");
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var user = await _context.Users.FindAsync(userId);
        return View(user);
    }
    [HttpPost]
    public async Task<IActionResult> UpdatePassword(string Password, string NewPassword, string ConfirmPassword)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        if (NewPassword != ConfirmPassword)
        {
            ModelState.AddModelError("", "Mật khẩu mới và xác nhận mật khẩu không khớp.");
            return View();
        }

        var changePasswordResult = await _userManager.ChangePasswordAsync(user, Password, NewPassword);
        if (!changePasswordResult.Succeeded)
        {
            foreach (var error in changePasswordResult.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return View();
        }

        await _signInManager.RefreshSignInAsync(user);
        TempData["SuccessMessage"] = "Mật khẩu của bạn đã được cập nhật thành công.";
        return RedirectToAction("Account", "Profile");
    }
    [HttpPost]
    public async Task<IActionResult> UpdateProfile(User model)
    {
        // Lấy thông tin người dùng hiện tại
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        // Cập nhật thông tin từ form vào người dùng hiện tại
        user.FullName = model.FullName;
        user.DateJoined = model.DateJoined;
        user.Email = model.Email;
        user.PhoneNumber = model.PhoneNumber;

        // Thực hiện cập nhật thông tin
        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            // Nếu cập nhật thành công, chuyển hướng đến trang hồ sơ
            TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công!";
            return RedirectToAction("Account", "Profile");
        }

        // Nếu có lỗi, hiển thị thông báo lỗi
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError("", error.Description);
        }

        // Trả về view với model chứa thông tin lỗi
        return View(user);
    }
    
}
