namespace EduCourse.Entities
{
    public class LessonProgress
    {
        public int LessonProgressID { get; set; }
        public int UserID { get; set; }
        public int LessonID { get; set; }
        public DateTime LastAccessed { get; set; }
        public double CompletionPercentage { get; set; } // e.g., 0-100%

        // Navigation properties
        public User User { get; set; }
        public Lesson Lesson { get; set; }
    }

}
