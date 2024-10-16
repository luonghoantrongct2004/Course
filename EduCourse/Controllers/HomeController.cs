using EduCourse.Data;
using EduCourse.Entities;
using EduCourse.Models;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public HomeController(ILogger<HomeController> logger, AppDbContext context, IMemoryCache cache, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _logger = logger;
            _context = context;
            _cache = cache;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public async Task<IActionResult> Search(string name, string categories = "", string instructors = "", int currentPage = 1)
        {
            const int pageSize = 10;

            // Tách danh mục và giảng viên từ chuỗi tham số
            var selectedCategories = !string.IsNullOrEmpty(categories) ? categories.Split(',') : Array.Empty<string>();
            var selectedInstructors = !string.IsNullOrEmpty(instructors) ? instructors.Split(',') : Array.Empty<string>();

            // Tạo query cơ bản
            var query = _context.Courses
                .Include(c => c.Chapters).ThenInclude(l => l.Lessons)
                .AsQueryable();

            // Lọc theo tên hoặc giá
            if (!string.IsNullOrWhiteSpace(name))
            {
                if (decimal.TryParse(name, out decimal price))
                {
                    query = query.Where(c => c.Price <= price);
                }
                else
                {
                    query = query.Where(c => c.Title.Contains(name)
                                           || c.Category.Name.Contains(name)
                                           || c.Author.FullName.Contains(name));
                }
            }

            // Lọc theo danh mục
            if (selectedCategories.Any())
            {
                query = query.Where(c => selectedCategories.Contains(c.Category.Name));
            }

            // Lọc theo giảng viên
            if (selectedInstructors.Any())
            {
                query = query.Where(c => selectedInstructors.Contains(c.Author.FullName));
            }

            // Tính toán tổng số kết quả và số trang
            var totalResults = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalResults / (double)pageSize);

            // Lấy các khóa học cho trang hiện tại
            var courses = await query
                .Include(c => c.Category)
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Truyền thông tin phân trang qua ViewData
            ViewData["CurrentPage"] = currentPage;
            ViewData["TotalPages"] = totalPages;
            ViewData["TotalResults"] = totalResults;

            var viewModel = new SearchViewModel
            {
                Courses = courses,
                Categories = await _context.Categories.ToListAsync(),
                Instructors = await _context.Users.ToListAsync(),
                CourseName = name ?? "",
                CategoryName = categories,
                InstructorName = instructors
            };

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var purchasedCourses = _context.Orders
                .Where(o => o.UserID == userId && o.PaymentStatus == "Completed")
                .SelectMany(o => o.OrderDetails.Select(od => od.CourseID))
                .ToList();
            ViewBag.PurchasedCourses = purchasedCourses;

            return View(viewModel);
        }


        public async Task<IActionResult> SearchFilter(string categories, string instructors)
        {
            var categoryList = categories?.Split(',') ?? new string[] { };
            var instructorList = instructors?.Split(',') ?? new string[] { };

            var query = _context.Courses
                .Include(c => c.Chapters).ThenInclude(l => l.Lessons)
                .AsQueryable();

            // Lọc theo danh mục
            if (categoryList.Any())
            {
                query = query.Where(c => categoryList.Contains(c.Category.Name));
            }

            // Lọc theo giảng viên
            if (instructorList.Any())
            {
                query = query.Where(c => instructorList.Contains(c.Author.FullName));
            }

            // Lấy danh sách khóa học đã lọc
            var filteredCourses = await query.Include(c => c.Category).ToListAsync();

            // Trả về một Partial View với danh sách khóa học đã lọc
            return View("_CourseListPartial", filteredCourses);
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
            var instructors = await _userManager.GetUsersInRoleAsync("Instructure");
            var viewModel = new HomeViewModel
            {
                Categories = categories,
                Instructors = instructors.ToList(),
                Courses = courses
            };
            return View(viewModel);
        }
        public async Task<IActionResult> Detail(int id)
        {
            var course = await _context.Courses
                .Include(c => c.UserCourses)
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