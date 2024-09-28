using EduCourse.Entities;

namespace EduCourse.Models;

public class ProfileViewModel
{
    public int EnrollCourse {  get; set; }
    public int Exam {  get; set; }
    public List<Order>? Orders { get; set; }
    public List<UserCourse>? UserCourses { get; set; }
    public List<StudentExam>? StudentExams { get; set; }
    public User? User { get; set; }
}
