namespace EduCourse.Entities
{
    public class Option
    {
        public int OptionID { get; set; }
        public int QuestionID { get; set; }
        public string OptionText { get; set; }
        public bool IsCorrect { get; set; } // True if this is the correct answer

        // Navigation properties
        public Question Question { get; set; }
    }
}