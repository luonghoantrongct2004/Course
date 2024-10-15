using EduCourse.Entities;

namespace EduCourse.Models;

public class ProfileViewModel
{
    public int EnrollCourse {  get; set; }
    public int Exam {  get; set; }
    public List<UserCourse>? UserCourses { get; set; }
    public List<StudentExam>? StudentExams { get; set; }
    public User? User { get; set; }
    public List<Order>? Orders { get; set; }
    public int TotalOrders { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public string? TimeFrame { get; set; }
    public int TotalPages { get; set; } = 0;
}
