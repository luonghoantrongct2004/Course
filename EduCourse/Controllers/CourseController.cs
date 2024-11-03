using EduCourse.Data;
using EduCourse.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EduCourse.Controllers;

public class CourseController : Controller
{
    private readonly AppDbContext _context;

    public CourseController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int id)
    {
        var course = await _context.Courses
              .Include(c => c.Chapters)
                .ThenInclude(l => l.Lessons)
                    .ThenInclude(q => q.Questions)
                        .ThenInclude(o => o.Options)
              .Include(a => a.Author)
              .Include(c => c.Category)
              .FirstOrDefaultAsync(c => c.CourseID == id && c.Status == true);

        return View(course);
    }
    [HttpPost]
    public JsonResult SaveProgress(int lessonID, double watchedPercentage)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // Update or create UserProgress for the given lesson
        var progress = _context.UserProgress
                               .FirstOrDefault(p => p.UserID == userId && p.LessonID == lessonID);

        if (progress == null)
        {
            progress = new UserProgress
            {
                UserID = userId,
                LessonID = lessonID,
                WatchedPercentage = watchedPercentage,
                CompletionStatus = watchedPercentage >= 100 ? "Completed" : "In Progress",
                LastWatchedTime = DateTime.Now,
                CompletionDate = watchedPercentage >= 100 ? DateTime.Now : (DateTime?)null
            };
            _context.UserProgress.Add(progress);
        }
        else
        {
            progress.WatchedPercentage = Math.Max(progress.WatchedPercentage, watchedPercentage);
            progress.CompletionStatus = progress.WatchedPercentage >= 100 ? "Completed" : "In Progress";
            progress.LastWatchedTime = DateTime.Now;

            if (progress.WatchedPercentage >= 100 && progress.CompletionStatus == "Completed" && progress.CompletionDate == null)
            {
                progress.CompletionDate = DateTime.Now;
            }
        }

        _context.SaveChanges();

        // Check if the entire course is completed after this progress update
        var lesson = _context.Lessons
                             .Include(l => l.Chapter)
                             .FirstOrDefault(l => l.LessonID == lessonID);

        if (lesson != null)
        {
            var chapter = lesson.Chapter;
            if (chapter != null)
            {
                var courseId = chapter.CourseID;

                // Check if all lessons in all chapters of the course are completed
                bool allLessonsCompleted = _context.Chapters
                    .Where(ch => ch.CourseID == courseId)
                    .SelectMany(ch => ch.Lessons)
                    .All(l => _context.UserProgress.Any(p => p.UserID == userId && p.LessonID == l.LessonID && p.CompletionStatus == "Completed"));

                if (allLessonsCompleted)
                {
                    // Generate a certificate for the completed course if not already created
                    var existingCertificate = _context.Certificate
                        .FirstOrDefault(c => c.UserID == userId && c.CourseID == courseId);

                    if (existingCertificate == null)
                    {
                        var certificate = new Certificate
                        {
                            UserID = userId,
                            CourseID = courseId,
                            IssueDate = DateTime.Now,
                        };

                        _context.Certificate.Add(certificate);
                        _context.SaveChanges();

                        // Optionally: Generate a certificate file
                        GenerateCertificateFile(certificate);
                    }
                }
            }
        }

        return Json(new { success = true });
    }

    private void GenerateCertificateFile(Certificate certificate)
    {
        var user = _context.Users.FirstOrDefault(u => u.Id == certificate.UserID);
        var course = _context.Courses.FirstOrDefault(c => c.CourseID == certificate.CourseID);

        if (user == null || course == null) return;

        string certificateContent = $"Certificate of Completion\n\n" +
                                     $"This certifies that {user.FullName}\n" +
                                     $"has successfully completed the course \"{course.Title}\"\n" +
                                     $"on {certificate.IssueDate.ToString("MMMM dd, yyyy")}.";

        string certificatePath = Path.Combine("wwwroot", "certificates", $"Certificate_{certificate.CertificateID}.txt");

        // Ensure the directory exists
        Directory.CreateDirectory(Path.GetDirectoryName(certificatePath));

        // Save the certificate content to a file
        System.IO.File.WriteAllText(certificatePath, certificateContent);
    }

    [HttpGet]
    public IActionResult DownloadCertificate(int certificateId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // Retrieve the certificate for the specific user
        var certificate = _context.Certificate
                                  .FirstOrDefault(c => c.CertificateID == certificateId && c.UserID == userId);

        if (certificate == null)
        {
            return NotFound(new { message = "Certificate not found." });
        }

        // Load the certificate file
        string certificatePath = Path.Combine("wwwroot", "certificates", $"Certificate_{certificate.CertificateID}.txt");

        if (!System.IO.File.Exists(certificatePath))
        {
            return NotFound(new { message = "Certificate file not found." });
        }

        // Return the file for download
        byte[] fileBytes = System.IO.File.ReadAllBytes(certificatePath);
        return File(fileBytes, "text/plain", $"Certificate_{certificate.CertificateID}.txt");
    }

}
