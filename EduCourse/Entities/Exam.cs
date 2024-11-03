namespace EduCourse.Entities;
public class Exam
{
    public int ExamID { get; set; }
    public string? Title { get; set; }
    public string AuthorID { get; set; }
    public User? Author { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public bool IsActive { get; set; } = true;

    // Mối quan hệ nhiều-nhiều giữa Exam và Question
    public ICollection<ExamQuestion>? ExamQuestions { get; set; } = new List<ExamQuestion>();

    // Thêm thuộc tính Điểm Liệt và Điểm Đạt
    public double CutoffScore { get; set; } // Điểm liệt
    public double PassScore { get; set; } // Điểm đạt
}
