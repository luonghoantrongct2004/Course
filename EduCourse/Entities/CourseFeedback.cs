namespace EduCourse.Entities
{
    public class CourseFeedback
    {
        public int CourseFeedbackID { get; set; }
        public int CourseID { get; set; }
        public int UserID { get; set; }
        public string FeedbackText { get; set; }
        public int Rating { get; set; } // e.g., 1 to 5

        // Navigation properties
        public Course Course { get; set; }
        public User User { get; set; }
    }
}
