using EduCourse.Areas.Admin.Models;
using EduCourse.Data;
using EduCourse.Entities;
using EduCourse.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Claims;

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

    [HttpGet]
    public IActionResult Index(int page = 1, int pageSize = 10)
    {
        // Get the total count of exams
        var totalExams = _context.Exams.Count();

        // Fetch the paginated exams with related data
        var exams = _context.Exams
                            .Include(a => a.Author)
                            .Include(e => e.ExamQuestions)
                                .ThenInclude(eq => eq.Question)
                                .ThenInclude(o => o.Options)
                            .OrderBy(e => e.CreatedDate)
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .ToList();

        // Tạo Dictionary để lưu thông tin về số lượng sinh viên đạt và chưa đạt cho từng Exam
        var examStatistics = new Dictionary<int, (int TotalPassed, int TotalFailed)>();

        foreach (var exam in exams)
        {
            // Lấy danh sách StudentExam liên quan đến Exam hiện tại
            var studentExams = _context.StudentExams
                                       .Where(se => se.ExamID == exam.ExamID)
                                       .ToList();

            // Đếm số sinh viên đạt và không đạt cho từng Exam
            int countPassed = studentExams.Count(se => se.Result == "Passed");
            int countFailed = studentExams.Count(se => se.Result == "Failed" || se.Result == "Failed (below cutoff)");

            // Lưu vào Dictionary với ExamID là key và Tuple (Passed, Failed) là value
            examStatistics[exam.ExamID] = (countPassed, countFailed);
        }

        // Set ViewData for pagination
        ViewData["TotalExams"] = totalExams;
        ViewData["CurrentPage"] = page;
        ViewData["PageSize"] = pageSize;

        // Sử dụng ViewBag để truyền dữ liệu thống kê
        ViewBag.ExamStatistics = examStatistics;

        // Trả về View với danh sách Exam
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
            return Redirect("/Home/Error404");

        return View(exam);
    }

    [HttpGet]
    public IActionResult Create()
    {
        var questions = _context.Questions
            .Include(o => o.Options)
            .Include(eq => eq.ExamQuestions)
            .ToList();
        ViewData["Questions"] = questions;

        return View();
    }


    [HttpPost]
    public IActionResult Create([FromForm] Exam newExam, [FromForm] List<int> SelectedQuestions)
    {
        if (newExam == null || SelectedQuestions == null || !SelectedQuestions.Any())
            return Json(new { success = false, message = "Dữ liệu chưa được chọn" });

        using (var transaction = _context.Database.BeginTransaction())
        {
            try
            {
                var exam = new Exam
                {
                    Title = newExam.Title,
                    AuthorID = newExam.AuthorID,
                    CreatedDate = DateTime.Now,
                    ExamQuestions = new List<ExamQuestion>()
                };

                _context.Exams.Add(exam);
                _context.SaveChanges();

                foreach (var questionId in SelectedQuestions)
                {
                    var existingExamQuestion = _context.ExamQuestions
                        .AsNoTracking()
                        .FirstOrDefault(eq => eq.ExamID == exam.ExamID && eq.QuestionID == questionId);

                    if (existingExamQuestion == null)
                    {
                        var examQuestion = new ExamQuestion
                        {
                            ExamID = exam.ExamID,
                            QuestionID = questionId
                        };
                        _context.ExamQuestions.Add(examQuestion);
                    }
                }

                _context.SaveChanges();
                transaction.Commit();

                return Json(new { success = true, message = "Đã tạo thành công bài thi" });
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return Json(new { success = false, message = ex.Message });
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
            return Redirect("/Home/Error404");
        }
        var questions = _context.Questions
            .Include(o => o.Options)
            .ToList();
        ViewData["Questions"] = questions;


        return View(exam); // Truyền trực tiếp đối tượng Exam vào View
    }

    [HttpPost]
    public IActionResult Edit(int id, ExamEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { success = false, message = "Invalid model state." });
        }

        var exam = _context.Exams
            .Include(e => e.ExamQuestions) // Include existing ExamQuestions
            .FirstOrDefault(e => e.ExamID == id);

        if (exam == null)
            return Redirect("/Home/Error404");

        // Update basic exam properties
        exam.Title = model.Title;
        exam.AuthorID = model.AuthorID;
        exam.PassScore = model.PassScore;
        exam.CutoffScore = model.CutoffScore;

        // Clear existing ExamQuestions
        exam.ExamQuestions.Clear();

        // Add new ExamQuestions based on SelectedQuestionIDs
        if (model.SelectedQuestionIDs != null)
        {
            foreach (var questionId in model.SelectedQuestionIDs)
            {
                exam.ExamQuestions.Add(new ExamQuestion
                {
                    ExamID = exam.ExamID,
                    QuestionID = questionId
                });
            }
        }

        _context.SaveChanges();
        return Json(new { success = true, message = "Exam updated successfully." });
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

    public IActionResult StudentExam(int page = 1, int pageSize = 10)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        IQueryable<StudentExam> query;

        // Determine query based on user role
        if (User.IsInRole("Instructure"))
        {
            // Filter by AuthorID for instructors
            query = _context.StudentExams
                .Include(e => e.Exam)
                .Include(s => s.ExamDetails)
                    .ThenInclude(q => q.Question)
                .Include(e => e.Exam.Author) // Include Author for filtering
                .Include(e => e.Student)
                .Where(s => s.Exam.AuthorID == userId)
                .OrderByDescending(d => d.ExamDate);
        }
        else if (User.IsInRole("Admin"))
        {
            // Admin sees all exams without filtering by AuthorID
            query = _context.StudentExams
                .Include(e => e.Exam)
                .Include(s => s.ExamDetails)
                  .ThenInclude(q => q.Question)
                .Include(e => e.Exam.Author)
                .Include(e => e.Student)
                .OrderByDescending(d => d.ExamDate);
        }
        else
        {
            // Redirect to error if user doesn't have proper roles
            return Redirect("/home/error500");
        }
      

        // Get total count of exams for pagination
        var totalExamsCount = query.Count();

        ViewData["TotalExams"] = totalExamsCount;
        ViewData["CurrentPage"] = page;
        ViewData["PageSize"] = pageSize;
        // Execute pagination
        var studentExams = query.Skip((page - 1) * pageSize)
                                .Take(pageSize)
                                .ToList();

        // Calculate total pages
        var totalPages = (int)Math.Ceiling((double)totalExamsCount / pageSize);

        // Prepare the view model
        var viewModel = new ProfileViewModel
        {
            CurrentPage = page,
            StudentExams = studentExams,
            PageSize = pageSize,
            TotalPages = totalPages
        };
        // Return the view with the data
        return View(viewModel);

    }

    public IActionResult StudentExamDetails(int id)
    {
        var exam = _context.StudentExams
           .Include(e => e.Exam)
           .Include(s => s.ExamDetails)
               .ThenInclude(q => q.Question)
                   .ThenInclude(o => o.Options)
           .FirstOrDefault(e => e.Id == id);
        return View(exam);
    }
    [HttpPost]
    public IActionResult ToggleActive(int id, bool isActive)
    {
        var exam = _context.Exams.FirstOrDefault(i => i.ExamID == id);
        if (exam == null)
            return Json(new { success = false, message = "Exam không tìm thấy" });

        // Nếu bài thi được chọn để kích hoạt
        if (isActive)
        {
            // Tắt trạng thái active của tất cả các bài thi khác
            var otherExams = _context.Exams.Where(e => e.ExamID != id).ToList();
            foreach (var item in otherExams)
            {
                item.IsActive = false;
            }
        }

        // Cập nhật trạng thái bài thi hiện tại
        exam.IsActive = isActive;
        _context.SaveChanges();

        return Json(new { success = true, message = "Đã cập nhật trạng thái thành công" });
    }

}
