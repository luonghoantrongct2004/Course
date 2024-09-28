using System.ComponentModel.DataAnnotations;

namespace EduCourse.Entities;

public class Course
{
    public int CourseID { get; set; }
    [Required(ErrorMessage = "Tên khóa học là bắt buộc.")]
    public string? Title { get; set; }
    [Required(ErrorMessage = "Mô tả khóa học là bắt buộc.")]
    public string? Description { get; set; }
    [Required(ErrorMessage = "Giá là bắt buộc.")]
    [Range(0, double.MaxValue, ErrorMessage = "Giá không hợp lệ.")]
    public decimal Price { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
    public int Discount { get; set; }
    public string? Image { get; set; }
    public string AuthorID { get; set; }
    [Required(ErrorMessage = "Thể loại là bắt buộc.")]
    public int CategoryID { get; set; }
    public User? Author { get; set; }
    public bool Status { get; set; } = true;

    public ICollection<Chapter> Chapters { get; set; } = new List<Chapter>();
    public Category? Category { get; set; }
    public ICollection<UserCourse>? UserCourses { get; set; }
}