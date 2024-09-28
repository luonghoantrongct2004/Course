using EduCourse.Data;
using EduCourse.Entities;
using EduCourse.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EduCourse.Controllers;

public class ProfileController : Controller
{
    private readonly AppDbContext _context;

    public ProfileController(AppDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        if (!User.Identity.IsAuthenticated)
        {
            return Redirect("/Auth/Login");
        }
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var countCourseEnrolled = _context.Orders.Where(o => o.UserID == userId).Count();
        var examCount = _context.StudentExams.Where(u => u.StudentID == userId).Count();
        var userCourses = _context.UserCourses
           .Where(uc => uc.UserID == userId)
           .Include(u => u.User)
           .Include(uc => uc.Course)
                .ThenInclude(c => c.Chapters)
                    .ThenInclude(l => l.Lessons)
           .ToList();
        var viewModel = new ProfileViewModel
        {
            EnrollCourse = countCourseEnrolled,
            Exam = examCount,
            User = _context.Users.FirstOrDefault(u => u.Id == userId),
            UserCourses = userCourses
        };

        return View(viewModel);
    }
    public IActionResult HistoryOrder()
    {
        if (!User.Identity.IsAuthenticated)
        {
            return Redirect("/Auth/Login");
        }
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var viewModel = new ProfileViewModel
        {
            User = _context.Users.FirstOrDefault(u => u.Id == userId),
            Orders = _context.Orders.Include(d => d.OrderDetails).ThenInclude(c => c.Course).Where(u => u.UserID == userId).ToList()
        };

        return View(viewModel);
    }
}
