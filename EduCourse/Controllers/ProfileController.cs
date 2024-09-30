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

}
