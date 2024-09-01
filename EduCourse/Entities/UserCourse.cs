namespace EduCourse.Entities
{
    public class UserCourse
    {
        public int UserCourseID { get; set; }
        public int UserID { get; set; }
        public int CourseID { get; set; }
        public DateTime EnrollDate { get; set; }
        public string Status { get; set; } // e.g., Active, Completed, Dropped

        // Navigation properties
        public User User { get; set; }

        public Course Course { get; set; }
    }
}