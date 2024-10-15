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

        // Get exam details from the database
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
            Score = 0 // Total score will be calculated
        };

        // Loop through each question and calculate the score
        foreach (var examQuestion in exam.ExamQuestions)
        {
            var question = examQuestion.Question;
            var questionId = question.QuestionID;
            var questionName = question.Content;
            var questionType = question.QuestionType;
            string result = "Incorrect";
            double questionScore = 0;

            if (!submittedAnswers.ContainsKey(questionId))
            {
                // If the question was not answered, continue
                continue;
            }

            var selectedOptions = submittedAnswers[questionId];

            if (question.QuestionType == "Single Choice")
            {
                var correctOption = question.Options.FirstOrDefault(o => o.IsCorrect == true);
                if (correctOption != null && selectedOptions.Contains(correctOption.OptionID))
                {
                    result = "Correct";
                    questionScore = pointsPerQuestion; // Full points for a correct answer
                }
            }
            else if (question.QuestionType == "Multiple Choice")
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
            else if (question.QuestionType == "Keyword")
            {
                var keywordAnswer = selectedOptions.FirstOrDefault().ToString();
                if (string.Equals(question.Keyword, keywordAnswer, StringComparison.OrdinalIgnoreCase))
                {
                    result = "Correct";
                    questionScore = pointsPerQuestion;
                }
            }

            totalScore += questionScore;

            // Save each question's details to StudentExamDetail
            var examDetail = new StudentExamDetail
            {
                QuestionName = questionName,
                QuestionType = questionType,
                Result = result,
                Score = questionScore,
                StudentExam = studentExam // Link to the parent StudentExam
            };

            _context.StudentExamDetails.Add(examDetail); // Add each question's detail
        }

        // Round the total score and save it to StudentExam
        studentExam.Score = (int)Math.Round(totalScore);
        _context.StudentExams.Add(studentExam);
        _context.SaveChanges();

        return Redirect("/home/StudentExam");
    }


}
