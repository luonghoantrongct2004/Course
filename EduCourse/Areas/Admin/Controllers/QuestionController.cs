using EduCourse.Areas.Admin.Models;
using EduCourse.Data;
using EduCourse.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduCourse.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class QuestionController : Controller
{
    private readonly AppDbContext _context;

    public QuestionController(AppDbContext context)
    {
        _context = context;
    }
    [HttpGet]
    public IActionResult Index(int page = 1, int pageSize = 10)
    {
        // Get the total count of questions
        var totalQuestions = _context.Questions.Count();

        // Fetch the paginated questions with related data
        var questions = _context.Questions
                                .Include(q => q.Options)
                                .Include(l => l.Lesson)
                                .OrderBy(q => q.QuestionID)  // Sort by any column (QuestionID in this case)
                                .Skip((page - 1) * pageSize)
                                .Take(pageSize)
                                .ToList();

        // Set ViewData for pagination
        ViewData["TotalQuestions"] = totalQuestions;
        ViewData["CurrentPage"] = page;
        ViewData["PageSize"] = pageSize;

        return View(questions);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();  // Trả về view để hiển thị form tạo câu hỏi
    }

    [HttpPost]
    public JsonResult Create(QuestionModel question, List<Option> options, string IsCorrect)
    {
        if (question == null || (question.QuestionType != "Keyword" && options == null))
        {
            return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
        }

        using (var transaction = _context.Database.BeginTransaction())
        {
            try
            {
                var questionEntity = new Question
                {
                    Content = question.Content,
                    QuestionType = question.QuestionType,
                    Keyword = question.Keyword,
                    CorrectAnswer = question.CorrectAnswer,
                };
                // Lưu câu hỏi trước để có QuestionID
                _context.Questions.Add(questionEntity);
                _context.SaveChanges();

                // Xử lý câu hỏi loại Single Choice
                if (question.QuestionType == "Single Choice")
                {
                    int correctIndex = int.Parse(IsCorrect);  // Convert IsCorrect into the correct answer index

                    List<Option> newOptions = new List<Option>();

                    for (int i = 0; i < options.Count; i++)
                    {
                        bool isCorrect = (i == correctIndex);
                        var newOption = new Option
                        {
                            Content = options[i].Content,
                            IsCorrect = isCorrect, // Set true or false based on correct index
                            QuestionID = questionEntity.QuestionID
                        };

                        // Log existing options for debugging purposes
                        var existingOption = _context.Options
                            .FirstOrDefault(o => o.QuestionID == questionEntity.QuestionID && o.Content == options[i].Content);

                        if (existingOption != null)
                        {
                            Console.WriteLine($"Option {options[i].Content} already exists for QuestionID {questionEntity.QuestionID}");
                        }
                        else
                        {
                            // Add the new option to the list
                            Console.WriteLine($"Adding option {options[i].Content} with IsCorrect = {isCorrect}");
                            newOptions.Add(newOption);
                        }
                    }

                    // Ensure new options are added to the context
                    if (newOptions.Count > 0)
                    {
                        _context.Options.AddRange(newOptions);
                        _context.SaveChanges();
                        Console.WriteLine("Options successfully saved.");
                    }
                    else
                    {
                        Console.WriteLine("No new options to add.");
                    }
                }

                // Xử lý câu hỏi loại Multiple Choice
                else if (question.QuestionType == "Multiple Choice")
                {
                    List<Option> newOptions = new List<Option>();

                    for (int i = 0; i < options.Count; i++)
                    {
                        var newOption = new Option
                        {
                            Content = options[i].Content,
                            IsCorrect = options[i].IsCorrect,
                            QuestionID = questionEntity.QuestionID
                        };

                        // Chỉ thêm Option mới nếu chưa tồn tại cho câu hỏi này
                        if (!_context.Options.Any(o => o.QuestionID == questionEntity.QuestionID && o.Content == options[i].Content))
                        {
                            newOptions.Add(newOption);
                        }
                    }

                    _context.Options.AddRange(newOptions);
                }
                // Xử lý câu hỏi loại Keyword
                else if (question.QuestionType == "Keyword")
                {
                    questionEntity.CorrectAnswer = question.CorrectAnswer;
                    _context.Entry(questionEntity).State = EntityState.Modified;
                    _context.SaveChanges();
                }

                // Save and commit transaction
                _context.SaveChanges();
                transaction.Commit();

                return Json(new { success = true, message = "Câu hỏi đã được tạo thành công." });
            }
            catch (Exception ex)
            {
                // Rollback transaction in case of error
                transaction.Rollback();
                return Json(new { success = false, message = "Đã xảy ra lỗi: " + ex.Message });
            }
        }
    }


    [HttpGet]
    public IActionResult Detail(int id)
    {
        var question = _context.Questions
            .Include(l => l.Lesson)
            .Include(q => q.Options)
            .FirstOrDefault(q => q.QuestionID == id);

        if (question == null)
            return NotFound("Question not found.");

        return View(question); 
    }

    [HttpGet]
    public IActionResult Update(int id)
    {
        var question = _context.Questions
             .Include(l => l.Lesson)
            .Include(o => o.Options)
            .FirstOrDefault(q => q.QuestionID == id);

        if (question == null)
            return Redirect("/Home/404");

        return View(question); 
    }
    [HttpPost]
    public IActionResult Update(Question question, int? CorrectOption)
    {
        if (!ModelState.IsValid)
        {
            return View(question);
        }

        var questionToUpdate = _context.Questions
            .Include(q => q.Options)
            .FirstOrDefault(q => q.QuestionID == question.QuestionID);

        if (questionToUpdate == null)
            return NotFound("Question not found.");

        // Update question details
        questionToUpdate.Content = question.Content;
        questionToUpdate.LessonID = question.LessonID;
        questionToUpdate.ShowTime = question.ShowTime;
        questionToUpdate.QuestionType = question.QuestionType;

        // Update for Keyword question type
        if (question.QuestionType == "Keyword")
        {
            questionToUpdate.CorrectAnswer = question.CorrectAnswer;
            questionToUpdate.Options.Clear();  // No options for keyword questions
        }
        else
        {
            // Update options for Single or Multiple Choice question types
            foreach (var option in question.Options)
            {
                var existingOption = questionToUpdate.Options.FirstOrDefault(o => o.OptionID == option.OptionID);
                if (existingOption != null)
                {
                    existingOption.Content = option.Content;

                    if (CorrectOption.HasValue && question.QuestionType == "Single Choice")
                    {
                        existingOption.IsCorrect = existingOption.OptionID == CorrectOption;
                    }
                    else if (question.QuestionType == "Multiple Choice")
                    {
                        existingOption.IsCorrect = option.IsCorrect;
                    }
                }
            }
        }

        _context.SaveChanges();

        return Redirect("/Admin/Question");
    }

    [HttpPost]
    public IActionResult DeleteQuestion(int id)
    {
        var question = _context.Questions
            .Include(q => q.Options)  // Include the related options
            .FirstOrDefault(q => q.QuestionID == id);

        if (question == null)
            return NotFound("Question not found.");

        _context.Questions.Remove(question);  // Options will be deleted automatically
        _context.SaveChanges();
        return Redirect("/Admin/Question");
    }

}
