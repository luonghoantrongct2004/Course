using EduCourse.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduCourse.Controllers;

public class CourseController : Controller
{
    private readonly AppDbContext _context;

    public CourseController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int id)
    {
        var course = await _context.Courses
              .Include(c => c.Chapters)
                .ThenInclude(l => l.Lessons)
                    .ThenInclude(q => q.Questions)
                        .ThenInclude(o => o.Options)
              .Include(a => a.Author)
              .Include(c => c.Category)
              .FirstOrDefaultAsync(c => c.CourseID == id);

        return View(course);
    }
}
