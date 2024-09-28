using EduCourse.Data;
using EduCourse.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace EduCourse.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class ExamController : Controller
{
    private readonly AppDbContext _context;

    public ExamController(AppDbContext context)
    {
        _context = context;
    }

    // GET: Lấy danh sách đề thi
    [HttpGet]
    public IActionResult Index()
    {
        var exams = _context.Exams
                                .Include(a => a.Author)
                                .Include(e => e.ExamQuestions)
                                  .ThenInclude(eq => eq.Question)
                                  .ThenInclude(o => o.Options)
                                  .ToList();

        return View(exams);
    }

    // GET: Lấy đề thi theo ID
    [HttpGet]
    public IActionResult Detail(int id)
    {
        var exam = _context.Exams
            .Include(a => a.Author)
            .Include(e => e.ExamQuestions)
                                 .ThenInclude(eq => eq.Question).ThenInclude(o => o.Options)
                                 .FirstOrDefault(e => e.ExamID == id);

        if (exam == null)
            return NotFound();

        return View(exam);
    }

    [HttpGet]
    public IActionResult Create()
    {
        var questions = _context.Questions
            .Include(o => o.Options)
            .ToList();
        ViewData["Questions"] = questions;

        return View();
    }


    [HttpPost]
    public IActionResult Create([FromForm] Exam newExam, [FromForm] List<int> SelectedQuestions)
    {
        // Check if exam data or selected questions are null or empty
        if (newExam == null || SelectedQuestions == null || !SelectedQuestions.Any())
            return Json(new { success = false, message = "Dữ liệu chưa được chọn" });

        using (var transaction = _context.Database.BeginTransaction()) // Start a database transaction
        {
            try
            {
                // Create a new Exam object and save it to the database
                var exam = new Exam
                {
                    Title = newExam.Title,
                    AuthorID = newExam.AuthorID,
                    CreatedDate = DateTime.Now,
                    ExamQuestions = new List<ExamQuestion>()  // Initialize the exam's ExamQuestions list
                };

                _context.Exams.Add(exam); // Add the exam to the database
                _context.SaveChanges(); // Save changes to generate the ExamID

                // Loop through each selected question
                foreach (var questionId in SelectedQuestions)
                {
                    // Check if the ExamQuestion already exists (i.e., if the question is already linked to the exam)
                    var existingExamQuestion = _context.ExamQuestions
                        .AsNoTracking() // Avoid tracking conflicts
                        .FirstOrDefault(eq => eq.ExamID == exam.ExamID && eq.QuestionID == questionId);

                    if (existingExamQuestion == null)
                    {
                        // If the question is not yet linked to the exam, create and add a new ExamQuestion
                        var examQuestion = new ExamQuestion
                        {
                            ExamID = exam.ExamID,   // Link to the current exam
                            QuestionID = questionId // Link to the selected question
                        };
                        _context.ExamQuestions.Add(examQuestion); // Add the exam-question relationship
                    }
                }

                _context.SaveChanges(); // Save all changes to the database
                transaction.Commit();   // Commit the transaction if everything is successful

                return Json(new { success = true, message = "Đã tạo thành công bài thi" }); // Success response
            }
            catch (Exception ex)
            {
                // Rollback the transaction in case of any errors
                transaction.Rollback();
                return Json(new { success = false, message = ex.Message }); // Error response
            }
        }
    }




    [HttpGet]
    public IActionResult Edit(int id)
    {
        var exam = _context.Exams
            .Include(e => e.ExamQuestions)
            .ThenInclude(eq => eq.Question)
            .ThenInclude(q => q.Options)
            .FirstOrDefault(e => e.ExamID == id);

        if (exam == null)
        {
            return NotFound();
        }
        var questions = _context.Questions
            .Include(o => o.Options)
            .ToList();
        ViewData["Questions"] = questions;


        return View(exam); // Truyền trực tiếp đối tượng Exam vào View
    }

    // POST: Cập nhật đề thi
    [HttpPost]
    public IActionResult Edit(int id, [FromBody] Exam updatedExam)
    {
        var exam = _context.Exams.FirstOrDefault(e => e.ExamID == id);

        if (exam == null)
            return NotFound();

        exam.Title = updatedExam.Title;
        exam.AuthorID = updatedExam.AuthorID;
        exam.ExamQuestions = updatedExam.ExamQuestions;

        _context.SaveChanges();
        return Redirect("/Admin/Exam");
    }

    [HttpDelete]
    public IActionResult DeleteExam(int id)
    {
        var exam = _context.Exams.FirstOrDefault(e => e.ExamID == id);

        if (exam == null)
            return NotFound(new { message = "Bài thi không tồn tại." });

        _context.Exams.Remove(exam);
        _context.SaveChanges();

        // Trả về phản hồi JSON thành công thay vì redirect
        return Ok(new { message = "Xóa thành công." });
    }

}
