using EduCourse.Entities;

namespace EduCourse.Models;

public class SearchViewModel
{
    public List<Course>? Courses { get; set; } = new List<Course> {};
    public List<Category>? Categories { get; set; } = new List<Category> ();
    public List<User>? Instructors {  get; set; } = new List<User> ();
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int TotalResults { get; set; }
    public string? CourseName { get; set; }
    public string? CategoryName { get; set; }
    public string? InstructorName { get; set; }
    public decimal? Price { get; set; }
}
