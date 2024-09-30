namespace EduCourse.Areas.Admin.Models
{
    public class ExamEditViewModel
    {
        public int ExamID { get; set; }
        public string Title { get; set; }
        public string AuthorID { get; set; }
        public List<int> SelectedQuestionIDs { get; set; }
    }

}
