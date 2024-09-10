namespace EduCourse.Entities
{
    public class ExamQuestion
    {
        public int ExamID { get; set; }
        public Exam? Exam { get; set; }

        public int QuestionID { get; set; }
        public Question? Question { get; set; }
    }
}
