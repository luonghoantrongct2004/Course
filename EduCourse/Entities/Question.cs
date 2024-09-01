namespace EduCourse.Entities
{
    public class Question
    {
        public int QuestionID { get; set; }
        public int QuizID { get; set; }
        public string Content { get; set; }
        public string QuestionType { get; set; } // Single Choice, Multiple Choice, Keyword

        // Navigation properties
        public Quiz? Quiz { get; set; }

        public ICollection<Option>? Options { get; set; }
    }
}