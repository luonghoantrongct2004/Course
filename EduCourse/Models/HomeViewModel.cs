using EduCourse.Entities;

namespace EduCourse.Models;

public class HomeViewModel
{
    public IEnumerable<Category>? Categories { get; set; }
    public IEnumerable<Course>? Courses { get; set; }
    public List<User>? Instructors { get; set; }
}
