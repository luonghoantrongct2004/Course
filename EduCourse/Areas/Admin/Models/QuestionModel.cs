namespace EduCourse.Areas.Admin.Models;

public class QuestionModel
{
    public string? Content { get; set; }
    public string? QuestionType { get; set; } // Single Choice, Multiple Choice, Keyword

    public string? CorrectAnswer { get; set; }

    public string? Keyword { get; set; }  // Để hỗ trợ cho câu hỏi dạng từ khóa
}
