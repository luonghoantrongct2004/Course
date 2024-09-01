namespace EduCourse.Entities
{
    public class StudentQuiz
    {
        public int StudentQuizID { get; set; }
        public int UserID { get; set; }
        public int QuizID { get; set; }
        public int Score { get; set; }
        public DateTime DateTaken { get; set; }

        // Navigation properties
        public User User { get; set; }

        public Quiz Quiz { get; set; }
    }
}