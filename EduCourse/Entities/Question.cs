namespace EduCourse.Entities
{
    public class Question
    {
        public int QuestionID { get; set; }
        public string? Content { get; set; }
        public string? QuestionType { get; set; } // Single Choice, Multiple Choice, Keyword

        public string? Text { get; set; }
        public string? CorrectAnswer { get; set; }
        public List<string> Choices { get; set; } = new List<string>();
        public double ShowTime { get; set; } // Thời gian trong video để hiển thị câu hỏi (tính bằng giây)
        public int LessonID { get; set; }
        public Lesson? Lesson { get; set; }

        public ICollection<Option>? Options { get; set; }
    }
}
