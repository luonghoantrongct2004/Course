namespace EduCourse.Entities
{
    public class Question
    {
        public int QuestionID { get; set; }
        public string? Content { get; set; }
        public string? QuestionType { get; set; } // Single Choice, Multiple Choice, Keyword

        public string? CorrectAnswer { get; set; }
        public List<string> Choices { get; set; } = new List<string>();
        public double ShowTime { get; set; } // Thời gian trong video để hiển thị câu hỏi (tính bằng giây)

        public string? Keyword { get; set; }  // Để hỗ trợ cho câu hỏi dạng từ khóa

        public int? LessonID { get; set; }
        public Lesson? Lesson { get; set; }

        public ICollection<Option>? Options { get; set; } = new List<Option>();
        public ICollection<ExamQuestion>? ExamQuestions { get; set; }
    }
}
