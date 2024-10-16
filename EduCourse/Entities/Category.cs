namespace EduCourse.Entities
{
    public class Category
    {
        public int CategoryID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Image { get; set; } = "/Lotus-icon.png";

        // Navigation properties
        public ICollection<Course>? Courses { get; set; }

        public ICollection<Lesson>? Lessons { get; set; }
        public ICollection<Quiz>? Quizzes { get; set; }
    }
}