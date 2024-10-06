using EduCourse.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduCourse.Controllers;

[Authorize]
public class ExamController : Controller
{
    private readonly AppDbContext _context;

    public ExamController(AppDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var topicExam = _context.Exams
            .Include(a => a.Author)
            .Include(eq => eq.ExamQuestions)
            .FirstOrDefault(a => a.IsActive);
        return View(topicExam);
    }
    public IActionResult PageExam(int id)
    {
        // Lấy tất cả các câu hỏi liên quan đến kỳ thi với ExamID = id
        var topicExam = _context.ExamQuestions
            .Include(eq => eq.Question)
                .ThenInclude(q => q.Options)
            .Include(q => q.Exam)
            .Where(a => a.ExamID == id)  // Sử dụng Where để lấy tất cả các câu hỏi liên quan
            .ToList(); // Chuyển đổi thành danh sách

        return View(topicExam); // Truyền danh sách các câu hỏi vào View
    }

}
