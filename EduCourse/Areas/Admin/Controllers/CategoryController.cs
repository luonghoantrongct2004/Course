using EduCourse.Data;
using EduCourse.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduCourse.Areas.Admin.Controllers;
[Area("Admin")]
[Authorize]
public class CategoryController : Controller
{
    private readonly AppDbContext _context;

    public CategoryController(AppDbContext context)
    {
        _context = context;
    }

    public IActionResult Index(int page = 1, int pageSize = 10)
    {
        // Get the total count of categories
        var totalCategories = _context.Categories.Count();

        // Fetch the paginated categories
        var categories = _context.Categories
            .OrderBy(c => c.CategoryID)  // Or any other property you want to order by
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // Set ViewData for pagination
        ViewData["TotalCategories"] = totalCategories;
        ViewData["CurrentPage"] = page;
        ViewData["PageSize"] = pageSize;

        return View(categories);
    }

    // GET: Admin/Category/Create
    public IActionResult Create()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Detail(int id)
    {
        var category = _context.Categories.FirstOrDefault(i => i.CategoryID == id);
        return View(category);
    }

    // POST: Admin/Category/Create
    [HttpPost]
    public async Task<IActionResult> Create([Bind("CategoryID, Name, Description")] Category category, IFormFile Image)
    {
        if (ModelState.IsValid)
        {
            try
            {
                if (Image != null && Image.Length > 0)
                {
                    // Lấy đường dẫn thư mục để lưu ảnh
                    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/categories");

                    // Đảm bảo thư mục tồn tại
                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }

                    // Đặt tên file là duy nhất
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(Image.FileName);
                    var filePath = Path.Combine(uploadPath, fileName);

                    // Lưu file lên thư mục
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await Image.CopyToAsync(fileStream);
                    }

                    // Lưu đường dẫn ảnh vào model
                    category.Image = "/categories/" + fileName;
                }
                _context.Add(category);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Danh mục đã được tạo thành công!" });
            }
            catch (Exception ex)
            {
                // Log the error (ex)
                return Json(new { success = false, message = "Có lỗi xảy ra khi tạo danh mục." });
            }
        }
        else
        {
            return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
        }
    }

    // GET: Admin/Category/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return Redirect("/Home/Error404");
        }

        var category = await _context.Categories.FindAsync(id);
        if (category == null)
        {
            return Redirect("/Home/Error404");
        }
        return View(category);
    }

    // POST: Admin/Category/Edit/5
    [HttpPost]
    public async Task<IActionResult> Edit(int id, [Bind("CategoryID, Name, Description")] Category category, IFormFile? file)
    {
        if (id != category.CategoryID)
        {
            return Redirect("/Home/Error404");
        }

        if (ModelState.IsValid)
        {
            try
            {
                if (file != null && file.Length > 0)
                {
                    // Lấy đường dẫn thư mục để lưu ảnh
                    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/categories");
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    var filePath = Path.Combine(uploadPath, fileName);

                    // Lưu file mới
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    // Xóa ảnh cũ nếu có
                    if (!string.IsNullOrEmpty(category.Image))
                    {
                        var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", category.Image.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    // Cập nhật đường dẫn ảnh mới
                    category.Image = "/categories/" + fileName;
                }
                _context.Update(category);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(category.CategoryID))
                {
                    return Redirect("/Home/Error404");
                }
                else
                {
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
                }
            }
            return Json(new { success = true, message = "Danh mục đã được tạo thành công!" });
        }
        return View(category);
    }
    // POST: Admin/Category/Delete/5
    [HttpDelete, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (!string.IsNullOrEmpty(category.Image))
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", category.Image.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        return Json(new { success = true, message = "Danh mục đã được tạo thành công!" });
    }

    private bool CategoryExists(int id)
    {
        return _context.Categories.Any(e => e.CategoryID == id);
    }


}
