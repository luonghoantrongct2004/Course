using System.ComponentModel.DataAnnotations;

namespace EduCourse.Entities;

public class Chapter
{
    public int ChapterID { get; set; }
    public int CourseID { get; set; }

    [Required(ErrorMessage = "Chương khóa học là bắt buộc.")]
    public string Title { get; set; }

    public Course? Course { get; set; }

    public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
}