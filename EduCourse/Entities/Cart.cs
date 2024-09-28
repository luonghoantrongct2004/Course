namespace EduCourse.Entities;

public class Cart
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public Guid UserId { get; set; }

    public User? User { get; set; }
    public Course? Course { get; set; }
}
