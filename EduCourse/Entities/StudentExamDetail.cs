namespace EduCourse.Entities
{
    public class StudentExamDetail
    {
        public int Id { get; set; }
        public int StudentExamId { get; set; }
        public StudentExam? StudentExam { get; set; }

        public string QuestionName { get; set; }
        public string QuestionType { get; set; }
        public string Result { get; set; } // "Correct" or "Incorrect"
        public double Score { get; set; } // Score for this specific question
    }

}
