namespace EduCourse.Entities
{
    public class StudentExamDetail
    {
        public int Id { get; set; }
        public int StudentExamId { get; set; }
        public StudentExam? StudentExam { get; set; }

        // Liên kết với Question để lấy chi tiết câu hỏi
        public int QuestionId { get; set; }
        public Question? Question { get; set; }

        public string QuestionType { get; set; } // Single Choice, Multiple Choice, Keyword
        public string Result { get; set; } // "Correct", "Incorrect", or "Partially Correct"
        public double Score { get; set; } // Điểm cho câu hỏi này

        // Lưu các lựa chọn đã chọn dưới dạng danh sách các ID đáp án
        public List<int> SelectedAnswers { get; set; } = new List<int>();
    }
}
