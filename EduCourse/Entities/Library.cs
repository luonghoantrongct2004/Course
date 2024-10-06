namespace EduCourse.Entities
{
    public class Library
    {
        public int LibraryID { get; set; }
        public int CourseID { get; set; }
        public int FileID { get; set; }
        public bool IsArchived { get; set; }

        // Navigation properties
        public Course? Course { get; set; }

        public File? File { get; set; }
    }
}