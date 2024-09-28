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

    public IActionResult Index()
    {
        var categories = _context.Categories.ToList();
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
    public async Task<IActionResult> Create([Bind("CategoryID, Name, Description")] Category category)
    {
        if (ModelState.IsValid)
        {
            try
            {
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
            return NotFound();
        }

        var category = await _context.Categories.FindAsync(id);
        if (category == null)
        {
            return NotFound();
        }
        return View(category);
    }

    // POST: Admin/Category/Edit/5
    [HttpPost]
    public async Task<IActionResult> Edit(int id, [Bind("CategoryID, Name, Description")] Category category)
    {
        if (id != category.CategoryID)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(category);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(category.CategoryID))
                {
                    return NotFound();
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
        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        return Json(new { success = true, message = "Danh mục đã được tạo thành công!" });
    }

    private bool CategoryExists(int id)
    {
        return _context.Categories.Any(e => e.CategoryID == id);
    }


}
