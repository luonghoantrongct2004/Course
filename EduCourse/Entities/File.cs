namespace EduCourse.Entities
{
    public class File
    {
        public int FileID { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string ContentType { get; set; }
        public DateTime UploadedDate { get; set; }
        public string UploadedByID { get; set; }

        // Navigation properties
        public User User { get; set; }
    }
}