using System.ComponentModel.DataAnnotations.Schema;

namespace EduCourse.Entities
{
    public class Library
    {
        public int LibraryID { get; set; }
        public string LibraryName { get; set; }
        public int CourseID { get; set; }
        public bool IsArchived { get; set; }
        public List<string> FilePaths { get; set; } = new List<string>();  // Changed to a list to store multiple file paths
        public DateTime UploadedDate { get; set; }
        public string UploadedByID { get; set; }

        // Navigation properties
        [ForeignKey("UploadedByID")]
        public User? User { get; set; }
        public Course? Course { get; set; }
    }
}
