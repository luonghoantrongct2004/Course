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
        public async Task<IActionResult> Search(string courseName, string categoryName, string instructorName, decimal? price, int currentPage = 1)
        {
            const int pageSize = 10;

            var query = _context.Courses
                .Include(c => c.Chapters)
                .AsQueryable();

            // Lọc theo tên khóa học
            if (!string.IsNullOrWhiteSpace(courseName))
            {
                query = query.Where(c => c.Title.Contains(courseName));
            }

            // Lọc theo tên danh mục
            if (!string.IsNullOrWhiteSpace(categoryName))
            {
                query = query.Where(c => c.Category.Name.Contains(categoryName));
            }

            // Lọc theo tên giảng viên
            if (!string.IsNullOrWhiteSpace(instructorName))
            {
                query = query.Where(c => c.Author.FullName.Contains(instructorName));
            }

            // Lọc theo giá
            if (price.HasValue)
            {
                query = query.Where(c => c.Price <= price);
            }

            // Tính tổng số kết quả
            var totalResults = await query.CountAsync();

            // Tính tổng số trang
            var totalPages = (int)Math.Ceiling(totalResults / (double)pageSize);

            // Lấy danh sách khóa học cho trang hiện tại
            var courses = await query
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewModel = new SearchViewModel
            {
                Courses = courses,
                Categories = await _context.Categories.ToListAsync(),
                Instructors = await _context.Users.ToListAsync(),
                CurrentPage = currentPage,
                TotalPages = totalPages,
                TotalResults = totalResults,
                CourseName = courseName,
                CategoryName = categoryName,
                InstructorName = instructorName,
                Price = price
            };

            return View(viewModel);
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
                    .ThenInclude(ch => ch.Lessons)
                .Include(a => a.Author)
                .Include(c => c.Category)
                .FirstOrDefaultAsync(c => c.CourseID == id); // Change this line

            if (course == null) // Check if the course is null
            {
                return NotFound(); // Return 404 if not found
            }

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