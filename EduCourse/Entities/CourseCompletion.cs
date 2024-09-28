namespace EduCourse.Entities
{
    public class CourseCompletion
    {
        public int CourseCompletionID { get; set; }
        public string UserID { get; set; }
        public int CourseID { get; set; }
        public DateTime CompletionDate { get; set; }
        public string CertificateURL { get; set; } // URL to a completion certificate

        // Navigation properties
        public User User { get; set; }
        public Course Course { get; set; }
    }

}
