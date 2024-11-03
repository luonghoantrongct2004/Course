using EduCourse.Data;
using EduCourse.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EduCourse.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CertificationController : Controller
    {
        private readonly AppDbContext _context;

        public CertificationController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(int page = 1, int pageSize = 10)
        {
            int totalCertificates = _context.Certificate.Count();

            var certificates = _context.Certificate
                .Include(c => c.User)
                .Include(c => c.Course)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewData["CurrentPage"] = page;
            ViewData["PageSize"] = pageSize;
            ViewData["TotalCertificates"] = totalCertificates;

            return View(certificates);
        }

        public IActionResult DownloadCertificate(int certificateId)
        {
            var certificate = _context.Certificate
                .Include(c => c.User)
                .Include(c => c.Course)
                .FirstOrDefault(c => c.CertificateID == certificateId);

            if (certificate == null)
            {
                return NotFound("Chứng chỉ không tồn tại");
            }
            string certificatePath = $"Certificates/{certificate.UserID}_{certificate.CourseID}_certificate.pdf";

            if (System.IO.File.Exists(certificatePath))
            {
                byte[] fileBytes = System.IO.File.ReadAllBytes(certificatePath);
                return File(fileBytes, "application/pdf", $"{certificate.UserID}_certificate.pdf");
            }
            else
            {
                return NotFound("Chứng chỉ PDF không tìm thấy");
            }
        }

        public bool CheckAllLessonsCompleted(string userId, int courseId)
        {
            // Lấy tất cả các chương thuộc khóa học
            var chapters = _context.Chapters.Where(ch => ch.CourseID == courseId).ToList();

            // Lấy tất cả bài học trong các chương của khóa học
            var lessons = new List<Lesson>();
            foreach (var chapter in chapters)
            {
                lessons.AddRange(_context.Lessons.Where(l => l.ChapterID == chapter.ChapterID));
            }

            var userProgress = _context.UserProgress.Where(p => p.UserID == userId).ToList();

            foreach (var lesson in lessons)
            {
                var progress = userProgress.FirstOrDefault(p => p.LessonID == lesson.LessonID);
                if (progress == null || progress.CompletionStatus != "Completed")
                {
                    return false;
                }
            }
            return true;
        }

        [HttpGet]
        public IActionResult GetCertificate(int courseId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Retrieve the certificate for the specific user and course
            var certificate = _context.Certificate
                                      .FirstOrDefault(c => c.UserID == userId && c.CourseID == courseId);

            if (certificate == null)
            {
                return NotFound(new { message = "Certificate not found or course not completed yet." });
            }

            // Load the certificate file
            string certificatePath = Path.Combine("wwwroot", "certificates", $"Certificate_{certificate.CertificateID}.txt");

            if (!System.IO.File.Exists(certificatePath))
            {
                return NotFound(new { message = "Certificate file not found." });
            }

            var fileContent = System.IO.File.ReadAllText(certificatePath);

            // Optionally, you can return the file for download or simply return the content
            return File(System.Text.Encoding.UTF8.GetBytes(fileContent), "text/plain", $"Certificate_{certificate.CertificateID}.txt");
        }

    }
}
