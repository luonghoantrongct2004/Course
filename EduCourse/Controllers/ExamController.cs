using EduCourse.Data;
using EduCourse.Entities;
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
            .Where(a => a.ExamID == id)
            .ToList(); // Chuyển đổi thành danh sách

        return View(topicExam); // Truyền danh sách các câu hỏi vào View
    }
    [HttpPost]
    public IActionResult SubmitExam(Dictionary<int, List<int>> submittedAnswers, int examId)
    {
        var studentId = User.Identity.Name;
        var student = _context.Users.FirstOrDefault(u => u.UserName == studentId);

        if (student == null)
        {
            return NotFound("Student not found");
        }

        // Lấy chi tiết của bài thi từ cơ sở dữ liệu
        var exam = _context.Exams
            .Include(e => e.ExamQuestions)
                .ThenInclude(eq => eq.Question)
                    .ThenInclude(q => q.Options)
            .FirstOrDefault(e => e.ExamID == examId);

        if (exam == null)
        {
            return NotFound("Exam not found");
        }

        int totalQuestions = exam.ExamQuestions.Count;
        if (totalQuestions == 0)
        {
            return BadRequest("No questions found for this exam.");
        }

        double pointsPerQuestion = 100.0 / totalQuestions;
        double totalScore = 0;

        var studentExam = new StudentExam
        {
            StudentID = student.Id,
            ExamID = exam.ExamID,
            ExamDate = DateTime.Now,
            Score = 0 // Tổng điểm sẽ được tính toán sau
        };

        foreach (var examQuestion in exam.ExamQuestions)
        {
            var question = examQuestion.Question;
            var questionId = question.QuestionID;
            var questionType = question.QuestionType;
            string result = "Incorrect";
            double questionScore = 0;

            if (!submittedAnswers.ContainsKey(questionId))
            {
                continue;
            }

            var selectedOptions = submittedAnswers[questionId]; // Danh sách ID đáp án đã chọn

            // Xử lý chấm điểm cho câu hỏi
            if (questionType == "Single Choice")
            {
                var correctOption = question.Options.FirstOrDefault(o => o.IsCorrect == true);
                if (correctOption != null && selectedOptions.Contains(correctOption.OptionID))
                {
                    result = "Correct";
                    questionScore = pointsPerQuestion;
                }
            }
            else if (questionType == "Multiple Choice")
            {
                var correctOptions = question.Options.Where(o => o.IsCorrect == true).Select(o => o.OptionID).ToList();
                var selectedCorrect = selectedOptions.Intersect(correctOptions).Count();
                var correctCount = correctOptions.Count;

                if (selectedCorrect > 0)
                {
                    questionScore = (selectedCorrect / (double)correctCount) * pointsPerQuestion;
                    result = (selectedCorrect == correctCount) ? "Correct" : "Partially Correct";
                }
            }
            else if (questionType == "Keyword")
            {
                var keywordAnswer = selectedOptions.FirstOrDefault().ToString(); // Lấy đáp án từ danh sách đã chọn
                if (string.Equals(question.Keyword, keywordAnswer, StringComparison.OrdinalIgnoreCase))
                {
                    result = "Correct";
                    questionScore = pointsPerQuestion;
                }
            }

            totalScore += questionScore;

            // Tạo đối tượng StudentExamDetail
            var examDetail = new StudentExamDetail
            {
                QuestionId = questionId, // Lưu QuestionID
                QuestionType = questionType,
                Result = result,
                Score = questionScore,
                SelectedAnswers = selectedOptions, // Lưu danh sách ID đáp án đã chọn
                StudentExam = studentExam
            };

            _context.StudentExamDetails.Add(examDetail); // Thêm chi tiết từng câu hỏi vào CSDL
        }

        // Tính tổng điểm và lưu lại
        studentExam.Score = (int)Math.Round(totalScore);

        // Kiểm tra xem thí sinh đậu hay rớt dựa vào điểm liệt và điểm đạt
        string passOrFail = "Failed";
        if (studentExam.Score < exam.CutoffScore)
        {
            passOrFail = "Failed (below cutoff)";
        }
        else if (studentExam.Score >= exam.PassScore)
        {
            passOrFail = "Passed";
        }

        studentExam.Result = passOrFail; // Lưu kết quả đậu/rớt vào đối tượng StudentExam
        _context.StudentExams.Add(studentExam);
        _context.SaveChanges();

        return Redirect($"/exam/details/{studentExam.Id}");
    }

    public IActionResult Details(int id)
    {
        var exam = _context.StudentExams
            .Include(e => e.Exam)
            .Include(s => s.ExamDetails)
                .ThenInclude(q => q.Question)
                    .ThenInclude(o => o.Options)
            .FirstOrDefault(e => e.Id == id);
        return View(exam);
    }

}
