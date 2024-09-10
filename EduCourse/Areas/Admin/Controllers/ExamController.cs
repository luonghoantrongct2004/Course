using EduCourse.Data;
using EduCourse.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduCourse.Areas.Admin.Controllers
{
    public class ExamController : Controller
    {
        private readonly AppDbContext _context;

        public ExamController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Lấy danh sách đề thi
        [HttpGet]
        public IActionResult GetExams()
        {
            var exams = _context.Exams.Include(e => e.ExamQuestions)
                                      .ThenInclude(eq => eq.Question)
                                      .ToList();

            return Json(new { success = true, data = exams });
        }

        // GET: Lấy đề thi theo ID
        [HttpGet]
        public IActionResult GetExam(int id)
        {
            var exam = _context.Exams.Include(e => e.ExamQuestions)
                                     .ThenInclude(eq => eq.Question)
                                     .FirstOrDefault(e => e.ExamID == id);

            if (exam == null)
                return Json(new { success = false, message = "Exam not found." });

            return Json(new { success = true, data = exam });
        }

        // POST: Tạo đề thi mới
        [HttpPost]
        public IActionResult CreateExam([FromBody] Exam newExam)
        {
            if (newExam == null)
                return Json(new { success = false, message = "Invalid exam data." });

            _context.Exams.Add(newExam);
            _context.SaveChanges();

            return Json(new { success = true, data = newExam });
        }

        // POST: Cập nhật đề thi
        [HttpPost]
        public IActionResult UpdateExam(int id, [FromBody] Exam updatedExam)
        {
            var exam = _context.Exams.FirstOrDefault(e => e.ExamID == id);

            if (exam == null)
                return Json(new { success = false, message = "Exam not found." });

            exam.Title = updatedExam.Title;
            exam.AuthorID = updatedExam.AuthorID;
            exam.ExamQuestions = updatedExam.ExamQuestions;

            _context.SaveChanges();
            return Json(new { success = true, data = exam });
        }

        // POST: Xóa đề thi
        [HttpPost]
        public IActionResult DeleteExam(int id)
        {
            var exam = _context.Exams.FirstOrDefault(e => e.ExamID == id);

            if (exam == null)
                return Json(new { success = false, message = "Exam not found." });

            _context.Exams.Remove(exam);
            _context.SaveChanges();
            return Json(new { success = true, message = "Exam deleted." });
        }
    }
}
