namespace EduCourse.Entities;

public class StudentExam
{
    public int Id { get; set; }
    public string StudentID { get; set; }
    public User? Student { get; set; }

    public int ExamID { get; set; }
    public Exam? Exam { get; set; }

    public DateTime ExamDate { get; set; }
    public int Score { get; set; } // Total score for the exam

    public List<StudentExamDetail> ExamDetails { get; set; } = new List<StudentExamDetail>(); // New
}
