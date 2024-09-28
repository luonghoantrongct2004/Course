using EduCourse.Data;
using EduCourse.Entities;
using EduCourse.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;

namespace EduCourse.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index(string paymentMessage)
        {
            if (!string.IsNullOrEmpty(paymentMessage))
            {
                ViewData["PaymentMessage"] = paymentMessage;
            }
            var categories = await _context.Categories.ToListAsync();
            var courses = await _context.Courses
                .Include(c => c.Chapters)
                    .ThenInclude(l => l.Lessons)
                .Include(a => a.Author)
                .Include(c => c.Category)
                .ToListAsync();
            var viewModel = new HomeViewModel
            {
                Categories = categories,
                Courses = courses
            };
            return View(viewModel);
        }
        public async Task<IActionResult> Detail(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Chapters)
                    .ThenInclude(l => l.Lessons)
                .Include(a => a.Author)
                .Include(c => c.Category)
                .FirstOrDefaultAsync(c => c.CategoryID == id);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var purchasedCourses = _context.Orders
            .Where(o => o.UserID == userId && o.PaymentStatus == "Completed")
             .SelectMany(o => o.OrderDetails.Select(od => od.CourseID))
             .ToList();

            ViewBag.PurchasedCourses = purchasedCourses;

            return View(course);
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public IActionResult Error404()
        {
            return View();
        }
        public IActionResult Error500()
        {
            return View();
        }
    }
}