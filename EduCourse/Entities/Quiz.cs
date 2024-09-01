namespace EduCourse.Entities
{
    public class Quiz
    {
        public int QuizID { get; set; }
        public int LessonID { get; set; }
        public string Title { get; set; }
        public int TimeLimit { get; set; } // In minutes
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int CategoryID { get; set; }  // Added CategoryID

        // Navigation properties
        public Lesson? Lesson { get; set; }

        public Category? Category { get; set; }  // Added Category
        public ICollection<Question>? Questions { get; set; }
    }
}