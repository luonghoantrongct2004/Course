using Microsoft.AspNetCore.Identity;

namespace EduCourse.Entities;

public class User: IdentityUser<int>
{
    public string FullName { get; set; }
    public DateTime DateJoined { get; set; }

    // Navigation properties
    public ICollection<Course>? Courses { get; set; }

    public ICollection<Payment>? Payments { get; set; }
    public ICollection<Exam>? CreatedExams { get; set; }
}