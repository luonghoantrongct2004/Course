namespace EduCourse.Entities
{
    public class Option
    {
        public int OptionID { get; set; }
        public string? Content { get; set; }
        public bool? IsCorrect { get; set; } // Indicates if this option is correct

        public int QuestionID { get; set; }
        public Question? Question { get; set; }
    }
}