namespace EduCourse.Entities
{
    public class Certificate
    {
        public int CertificateID { get; set; }
        public string UserID { get; set; } // ID người dùng
        public int CourseID { get; set; } // ID khóa học
        public DateTime IssueDate { get; set; } // Ngày cấp chứng chỉ

        public User? User { get; set; }
        public Course? Course { get; set; }
    }
}
