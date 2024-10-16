using EduCourse.Data;
using EduCourse.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace EduCourse.Areas.Admin.Controllers;

[Area("Admin")]
public class LibaryController : Controller
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public LibaryController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }

    public IActionResult Index(int page = 1, int pageSize = 10)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        // Get the total count of categories
        var totalLibraries = _context.Libraries.Count();
        List<Library> categories;

        if (User.IsInRole("Instructure"))
        {
            categories = _context.Libraries
             .Include(u => u.User)
             .Include(c => c.Course)
             .OrderByDescending(c => c.UploadedDate)
             .Where(u => u.UploadedByID == userId)
             .Skip((page - 1) * pageSize)
             .Take(pageSize)
             .ToList();
        }
        else if (User.IsInRole("Admin"))
        {
            categories = _context.Libraries
             .Include(u => u.User)
             .Include(c => c.Course)
             .OrderByDescending(c => c.UploadedDate)
             .Skip((page - 1) * pageSize)
             .Take(pageSize)
             .ToList();
        }
        else
        {
            return Redirect("/home/error404");
        }
        // Set ViewData for pagination
        ViewData["TotalLibraries"] = totalLibraries;
        ViewData["CurrentPage"] = page;
        ViewData["PageSize"] = pageSize;
        return View(categories);
    }
    public async Task<IActionResult> Delete(int id)
    {
        // Tìm thư viện theo id
        var library = await _context.Libraries.FindAsync(id);
        if (library == null)
        {
            return NotFound(new { message = "Thư viện không tồn tại." });
        }

        try
        {
            // Xóa từng file vật lý từ danh sách FilePaths
            if (library.FilePaths != null && library.FilePaths.Any())
            {
                foreach (var file in library.FilePaths)
                {
                    var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "libraries", file);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath); // Xóa file từ thư mục vật lý
                    }
                }
            }

            // Xóa bản ghi từ cơ sở dữ liệu
            _context.Libraries.Remove(library);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Xóa thành công." });
        }
        catch (Exception ex)
        {
            // Log lỗi nếu cần (tuỳ chọn)
            return StatusCode(500, new { message = "Đã xảy ra lỗi trong quá trình xóa." });
        }
    }

}
