using EduCourse.Data;
using EduCourse.Entities;
using EduCourse.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Stripe;
using System.Diagnostics;
using System.Security.Claims;

namespace EduCourse.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<HomeController> _logger;
        private readonly IMemoryCache _cache;

        public HomeController(ILogger<HomeController> logger, AppDbContext context, IMemoryCache cache)
        {
            _logger = logger;
            _context = context;
            _cache = cache;
        }
        public async Task<IActionResult> Search(string name, int currentPage = 1)
        {
            const int pageSize = 10;
            var query = _context.Courses
                .Include(c => c.Chapters).ThenInclude(l => l.Lessons)
                .AsQueryable();

            // Check if 'name' is provided
            if (!string.IsNullOrWhiteSpace(name))
            {
                // Try parsing the input as a price (decimal)
                if (decimal.TryParse(name, out decimal price))
                {
                    // If parsing is successful, filter by price
                    query = query.Where(c => c.Price <= price);
                }
                else
                {
                    // Otherwise, search by course name, category name, or instructor name
                    query = query.Where(c => c.Title.Contains(name)
                                          || c.Category.Name.Contains(name)
                                          || c.Author.FullName.Contains(name));
                }
            }

            // Calculate total results and total pages
            var totalResults = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalResults / (double)pageSize);

            // Get the courses for the current page
            var courses = await query
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Create a view model
            var viewModel = new SearchViewModel
            {
                Courses = courses,
                Categories = await _context.Categories.ToListAsync(),
                Instructors = await _context.Users.ToListAsync(),
                CurrentPage = currentPage,
                TotalPages = totalPages,
                TotalResults = totalResults,
                CourseName = name,
                CategoryName = name,
                InstructorName = name,
                Price = decimal.TryParse(name, out _) ? (decimal?)Convert.ToDecimal(name) : null
            };
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var purchasedCourses = _context.Orders
                .Where(o => o.UserID == userId && o.PaymentStatus == "Completed")
                .SelectMany(o => o.OrderDetails.Select(od => od.CourseID))
                .ToList();
            ViewBag.PurchasedCourses = purchasedCourses;
            return View(viewModel);
        }


        public async Task<IActionResult> Index(string paymentMessage)
        {
            var categoryList = _context.Categories.ToList();
            HttpContext.Session.SetString("CategoryList", JsonConvert.SerializeObject(categoryList));
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
                return Redirect("/Home/Error404"); // Return 404 if not found
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

        public IActionResult StudentExam(int page = 1, int pageSize = 10)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var student = _context.Users.FirstOrDefault(u => u.Id == userId);

            if (student == null)
            {
                return NotFound("Student not found");
            }
            var query = _context.StudentExams
                .Include(s => s.ExamDetails)
                .Include(e => e.Exam)
                .Include(e => e.Student)
                .OrderByDescending(d => d.ExamDate)
                .Where(s => s.StudentID == userId).ToList();
            var totalExam = query.ToList();
            var studentExams = totalExam.Skip((page - 1) * pageSize)
                                    .Take(pageSize)
                                    .ToList();
            var viewModel = new ProfileViewModel
            {
                User = student,
                CurrentPage = page,
                StudentExams = studentExams,
                PageSize = pageSize
            };
            return View(viewModel);
        }
    }
}