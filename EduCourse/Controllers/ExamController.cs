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
            .Where(a => a.ExamID == id)  // Sử dụng Where để lấy tất cả các câu hỏi liên quan
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

        // Lấy thông tin kỳ thi từ database
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

        // Điểm cho mỗi câu hỏi
        double pointsPerQuestion = 100.0 / totalQuestions;
        double totalScore = 0;

        // Duyệt qua từng câu hỏi của kỳ thi
        foreach (var examQuestion in exam.ExamQuestions)
        {
            var question = examQuestion.Question;
            var questionId = question.QuestionID;

            // Kiểm tra xem người dùng đã trả lời câu hỏi này chưa
            if (!submittedAnswers.ContainsKey(questionId))
            {
                continue; // Bỏ qua nếu câu hỏi chưa được trả lời
            }

            var selectedOptions = submittedAnswers[questionId];

            // Logic tính điểm dựa trên loại câu hỏi
            if (question.QuestionType == "Single Choice")
            {
                var correctOption = question.Options.FirstOrDefault(o => o.IsCorrect == true);
                if (correctOption != null && selectedOptions.Contains(correctOption.OptionID))
                {
                    totalScore += pointsPerQuestion; // Cộng điểm nếu đáp án đúng
                }
            }
            else if (question.QuestionType == "Multiple Choice")
            {
                // Kiểm tra số lượng đáp án đúng và người dùng có chọn đúng hết không
                var correctOptions = question.Options.Where(o => o.IsCorrect == true).Select(o => o.OptionID).ToList();
                var selectedCorrect = selectedOptions.Intersect(correctOptions).Count(); // Số đáp án đúng mà người dùng đã chọn
                var correctCount = correctOptions.Count; // Tổng số đáp án đúng

                // Tính phần điểm dựa trên số đáp án đúng đã chọn
                if (selectedCorrect > 0)
                {
                    double questionScore = (selectedCorrect / (double)correctCount) * pointsPerQuestion;
                    totalScore += questionScore; // Cộng điểm theo phần trăm số đáp án đúng
                }
            }
            else if (question.QuestionType == "Keyword")
            {
                var keywordAnswer = selectedOptions.FirstOrDefault().ToString();
                if (string.Equals(question.Keyword, keywordAnswer, StringComparison.OrdinalIgnoreCase))
                {
                    totalScore += pointsPerQuestion; // Cộng điểm nếu từ khóa đúng
                }
            }
        }

        // Làm tròn tổng điểm đến số nguyên gần nhất
        totalScore = Math.Round(totalScore);

        // Lưu kết quả thi vào bảng StudentExam
        var studentExam = new StudentExam
        {
            StudentID = student.Id,
            ExamID = exam.ExamID,
            ExamDate = DateTime.Now,
            Score = (int)totalScore
        };

        _context.StudentExams.Add(studentExam);
        _context.SaveChanges();

        // Chuyển hướng tới trang kết quả hoặc thông báo hoàn thành
        return Redirect("/profile/StudentExam");
    }

}
