using Microsoft.AspNetCore.Identity;

namespace EduCourse.Entities;

public class User: IdentityUser
{
    public string FullName { get; set; }
    public string Image {  get; set; } = "/Lotus-icon.png";
    public DateTime DateJoined { get; set; }
    public string Status { get; set; } = "Active";
    public string? Address { get; set; }

    public ICollection<Course>? Courses { get; set; }

    public ICollection<Payment>? Payments { get; set; }
    public ICollection<Exam>? CreatedExams { get; set; }
}