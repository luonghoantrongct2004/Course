using EduCourse.Data;
using EduCourse.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduCourse.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class Dashboard : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly AppDbContext _context;

    public Dashboard(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, AppDbContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        // Lấy danh sách người dùng theo từng vai trò
        var students = await _userManager.GetUsersInRoleAsync("Student");
        var instructors = await _userManager.GetUsersInRoleAsync("Instructor");
        var admin = await _userManager.GetUsersInRoleAsync("Admin");

        // Lấy những người dùng không thuộc vai trò Student hoặc Instructor
        var allUsers = _userManager.Users.ToList();

        // Thống kê số lượng người dùng theo từng tháng
        var studentCountByMonth = new int[12];
        var instructorCountByMonth = new int[12];
        var otherCountByMonth = new int[12];

        // Duyệt qua tất cả người dùng để phân loại theo tháng
        foreach (var user in students)
        {
            int monthJoined = user.DateJoined.Month - 1; // Trừ 1 vì mảng bắt đầu từ 0
            studentCountByMonth[monthJoined]++;
        }

        foreach (var user in instructors)
        {
            int monthJoined = user.DateJoined.Month - 1;
            instructorCountByMonth[monthJoined]++;
        }

        foreach (var user in admin)
        {
            int monthJoined = user.DateJoined.Month - 1;
            otherCountByMonth[monthJoined]++;
        }

        // Tạo dữ liệu biểu đồ cho 12 tháng
        var chartData = new
        {
            students = studentCountByMonth,
            instructors = instructorCountByMonth,
            others = otherCountByMonth
        };

        // Truyền dữ liệu biểu đồ qua ViewBag
        ViewBag.ChartData = chartData;
        decimal earning = _context.Orders.Sum(t => t.TotalAmount);
        ViewData["Student"] = students.ToList();
        ViewData["Course"] = _context.Courses.Include(u => u.UserCourses).ToList();
        ViewData["Exam"] = _context.Exams.ToList();
        ViewData["Order"] = earning;
        ViewData["Orders"] = _context.Orders.Include(d => d.OrderDetails).Include(u => u.User).ToList();

        return View();
    }
    [HttpGet]
    public async Task<IActionResult> GetChartDataByYear(int year)
    {
        // Lấy danh sách người dùng và các dữ liệu khác theo năm
        // Lấy tất cả người dùng trong năm đã chọn
        var usersInYear = _userManager.Users.Where(u => u.DateJoined.Year == year).ToList();

        // Lọc học sinh
        var students = 0;
        foreach (var user in usersInYear)
        {
            if (await _userManager.IsInRoleAsync(user, "Student"))
            {
                students++;
            }
        }

        // Lọc giáo viên
        var instructors = 0;
        foreach (var user in usersInYear)
        {
            if (await _userManager.IsInRoleAsync(user, "Instructor"))
            {
                instructors++;
            }
        }

        // Lọc quản trị viên
        var admins = 0;
        foreach (var user in usersInYear)
        {
            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                admins++;
            }
        }


        // Trả về dữ liệu dưới dạng JSON
        var chartData = new
        {
            students,
            instructors,
            others = admins
        };

        return Json(chartData);
    }


}
