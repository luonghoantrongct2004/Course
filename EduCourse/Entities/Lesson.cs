using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace EduCourse.Entities;

public class Lesson
{
    public int LessonID { get; set; }
    public int ChapterID { get; set; }
    [Required(ErrorMessage = "Tên bài học là bắt buộc.")]
    public string Title { get; set; }
    [Required(ErrorMessage = "Mô tả bài học là bắt buộc.")]
    public string Description { get; set; }
    public string? VideoURL { get; set; }
    public string ContentType { get; set; } = "Video"; // Video, PDF, Text, etc.
    public double Duration { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; } = DateTime.Now;

    [JsonIgnore]
    public Chapter? Chapter { get; set; }

    public List<Question>? Questions { get; set; }
}