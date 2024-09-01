using EduCourse.Data;
using EduCourse.Entities;
using MediaToolkit;
using MediaToolkit.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Drawing.Printing;

namespace EduCourse.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class CourseController : Controller
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public CourseController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }
    public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
    {
        var totalCourses = await _context.Courses.CountAsync();

        var courses = await _context.Courses
            .Include(p => p.Author)
            .Include(c => c.Category)
            .Include(c => c.Chapters)
                .ThenInclude(l => l.Lessons)
            .OrderBy(c => c.CreatedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewData["TotalCourses"] = totalCourses;
        ViewData["CurrentPage"] = page;
        ViewData["PageSize"] = pageSize;

        return View(courses);
    }

    public IActionResult Detail(int id)
    {
        var course = _context.Courses
            .Include(p => p.Author)
            .Include(c => c.Category)
            .Include(c => c.Chapters)
                .ThenInclude(l => l.Lessons)
            .FirstOrDefault(i => i.CourseID == id);

        if (course == null)
        {
            return NotFound();
        }

        return View(course);
    }


    public IActionResult Create()
    {
        ViewData["Categories"] = new SelectList(_context.Categories, "CategoryID", "Name");
        return View();
    }
    public async Task<double> GetVideoDurationAsync(string filePath)
    {
        try
        {
            var inputFile = new MediaFile { Filename = filePath };

            using (var engine = new Engine())
            {
                await Task.Run(() => engine.GetMetadata(inputFile));
            }

            if (inputFile.Metadata == null)
            {
                throw new Exception("Metadata could not be retrieved. The file may be corrupt or the format may not be supported.");
            }

            var duration = inputFile.Metadata.Duration.TotalSeconds;
            return duration;
        }
        catch (Exception ex)
        {
            throw new Exception($"Exception in GetVideoDurationAsync: {ex.Message}", ex);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create(Course course, IFormFile file, List<Chapter> chapters)
    {
        ViewData["Categories"] = new SelectList(_context.Categories, "CategoryID", "Name", course.CategoryID);

        if (ModelState.IsValid)
        {
            if (file != null)
            {
                var fileExtension = Path.GetExtension(file.FileName);
                var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                course.Image = $"/uploads/{uniqueFileName}";
            }

            if (chapters != null)
            {
                var chapterList = new List<Chapter>();
                for (int chapterIndex = 0; chapterIndex < chapters.Count; chapterIndex++)
                {
                    var chapter = chapters[chapterIndex];
                    var newChapter = new Chapter
                    {
                        ChapterID = chapter.ChapterID,
                        Title = chapter.Title,
                        Lessons = new List<Lesson>()
                    };

                    for (int lessonIndex = 0; lessonIndex < chapter.Lessons.Count; lessonIndex++)
                    {
                        var lesson = chapter.Lessons.ElementAt(lessonIndex);

                        var videoFileKey = $"Chapters[{chapterIndex}].Lessons[{lessonIndex}].VideoFile";
                        var videoFile = Request.Form.Files.FirstOrDefault(f => f.Name == videoFileKey);

                        if (videoFile != null)
                        {
                            var videoFileExtension = Path.GetExtension(videoFile.FileName);
                            var uniqueVideoFileName = Guid.NewGuid().ToString() + videoFileExtension;
                            var videoFilePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", uniqueVideoFileName);

                            using (var videoStream = new FileStream(videoFilePath, FileMode.Create))
                            {
                                await videoFile.CopyToAsync(videoStream);
                            }

                            lesson.VideoURL = $"/uploads/{uniqueVideoFileName}";

                            var durationString = await GetVideoDurationAsync(videoFilePath);
                            lesson.Duration = Convert.ToDouble(durationString);
                            lesson.CreatedDate = DateTime.Now;
                        }

                        lesson.Chapter = newChapter;
                        newChapter.Lessons.Add(lesson);
                    }
                    chapterList.Add(newChapter);
                }

                course.Chapters = chapterList;
            }

            course.CreatedDate = DateTime.Now;
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            return Redirect("/Admin/Course");
        }

        return View();
    }

    public IActionResult Edit(int id)
    {
        var course = _context.Courses
            .Include(c => c.Chapters)
            .ThenInclude(l => l.Lessons)
            .FirstOrDefault(c => c.CourseID == id);

        if (course == null)
        {
            return NotFound();
        }

        ViewData["Categories"] = new SelectList(_context.Categories, "CategoryID", "Name");
        return View(course);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int CourseID, IFormFile? file, Course course)
    {
        if (CourseID != course.CourseID)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Handle course image upload
                    if (file != null)
                    {
                        var fileExtension = Path.GetExtension(file.FileName);
                        var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        course.Image = $"/uploads/{uniqueFileName}";
                    }
                    else
                    {
                        var existingCourse = await _context.Courses.AsNoTracking()
                                                  .FirstOrDefaultAsync(c => c.CourseID == course.CourseID);
                        if (existingCourse != null)
                        {
                            course.Image = existingCourse.Image;
                        }
                    }

                    // Update course in the database
                    _context.Update(course);
                    await _context.SaveChangesAsync();

                    // Handle chapters and lessons
                    foreach (var chapter in course.Chapters)
                    {
                        if (chapter.ChapterID != 0)
                        {
                            var existingChapter = await _context.Chapters
                                .Include(c => c.Lessons)
                                .FirstOrDefaultAsync(c => c.ChapterID == chapter.ChapterID);

                            if (existingChapter != null)
                            {
                                _context.Entry(existingChapter).CurrentValues.SetValues(chapter);

                                foreach (var lesson in chapter.Lessons)
                                {
                                    if (lesson.LessonID == 0)
                                    {
                                        lesson.CreatedDate = DateTime.Now;
                                        lesson.UpdatedDate = DateTime.Now;

                                        var uploadedFile = Request.Form.Files[$"Chapters[{course.Chapters.ToList().IndexOf(chapter)}].Lessons[{chapter.Lessons.ToList().IndexOf(lesson)}].VideoFile"];
                                        if (uploadedFile != null && uploadedFile.Length > 0)
                                        {
                                            var videoExtension = Path.GetExtension(uploadedFile.FileName);
                                            var uniqueVideoFileName = Guid.NewGuid().ToString() + videoExtension;
                                            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "videos");
                                            var videoFilePath = Path.Combine(uploadsFolder, uniqueVideoFileName);

                                            if (!Directory.Exists(uploadsFolder))
                                            {
                                                Directory.CreateDirectory(uploadsFolder);
                                            }

                                            using (var stream = new FileStream(videoFilePath, FileMode.Create))
                                            {
                                                await uploadedFile.CopyToAsync(stream);
                                            }
                                            lesson.VideoURL = $"/videos/{uniqueVideoFileName}";
                                            var durationString = await GetVideoDurationAsync(videoFilePath);
                                            lesson.Duration = Convert.ToDouble(durationString);
                                        }

                                        existingChapter.Lessons.Add(lesson);
                                    }
                                    else
                                    {
                                        var existingLesson = existingChapter.Lessons
                                            .FirstOrDefault(l => l.LessonID == lesson.LessonID);

                                        if (existingLesson != null)
                                        {
                                            var uploadedFile = Request.Form.Files[$"Chapters[{course.Chapters.ToList().IndexOf(chapter)}].Lessons[{chapter.Lessons.ToList().IndexOf(lesson)}].VideoFile"];
                                            if (uploadedFile != null && uploadedFile.Length > 0)
                                            {
                                                var videoExtension = Path.GetExtension(uploadedFile.FileName);
                                                var uniqueVideoFileName = Guid.NewGuid().ToString() + videoExtension;
                                                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "videos");
                                                var videoFilePath = Path.Combine(uploadsFolder, uniqueVideoFileName);

                                                if (!Directory.Exists(uploadsFolder))
                                                {
                                                    Directory.CreateDirectory(uploadsFolder);
                                                }

                                                using (var stream = new FileStream(videoFilePath, FileMode.Create))
                                                {
                                                    await uploadedFile.CopyToAsync(stream);
                                                }
                                                lesson.VideoURL = $"/videos/{uniqueVideoFileName}";
                                                var durationString = await GetVideoDurationAsync(videoFilePath);
                                                lesson.Duration = Convert.ToDouble(durationString);
                                            }
                                            else
                                            {
                                                lesson.VideoURL = existingLesson.VideoURL;
                                            }

                                            lesson.UpdatedDate = DateTime.Now;
                                            _context.Entry(existingLesson).CurrentValues.SetValues(lesson);
                                        }
                                    }
                                }

                                await _context.SaveChangesAsync();  // Save changes after updating lessons
                            }
                        }
                        else
                        {
                            // Handle new chapters and lessons
                            chapter.CourseID = course.CourseID;  // Ensure the CourseID is set correctly

                            foreach (var lesson in chapter.Lessons)
                            {
                                lesson.CreatedDate = DateTime.Now;
                                lesson.UpdatedDate = DateTime.Now;

                                var uploadedFile = Request.Form.Files[$"Chapters[{course.Chapters.ToList().IndexOf(chapter)}].Lessons[{chapter.Lessons.ToList().IndexOf(lesson)}].VideoFile"];
                                if (uploadedFile != null && uploadedFile.Length > 0)
                                {
                                    var videoExtension = Path.GetExtension(uploadedFile.FileName);
                                    var uniqueVideoFileName = Guid.NewGuid().ToString() + videoExtension;
                                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "videos");
                                    var videoFilePath = Path.Combine(uploadsFolder, uniqueVideoFileName);

                                    if (!Directory.Exists(uploadsFolder))
                                    {
                                        Directory.CreateDirectory(uploadsFolder);
                                    }

                                    using (var stream = new FileStream(videoFilePath, FileMode.Create))
                                    {
                                        await uploadedFile.CopyToAsync(stream);
                                    }
                                    lesson.VideoURL = $"/videos/{uniqueVideoFileName}";
                                    var durationString = await GetVideoDurationAsync(videoFilePath);
                                    lesson.Duration = Convert.ToDouble(durationString);
                                }
                            }

                            _context.Chapters.Add(chapter);  // Add the new chapter to the context
                            await _context.SaveChangesAsync();  // Save changes immediately after adding the new chapter
                        }
                    }

                    await transaction.CommitAsync();
                }
                catch (DbUpdateException ex)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", $"An error occurred while updating the course: {ex.InnerException?.Message}");
                    ViewData["Categories"] = new SelectList(_context.Categories, "CategoryID", "Name", course.CategoryID);
                    return View(course);
                }
            }

            return Redirect("/Admin/Course");
        }

        ViewData["Categories"] = new SelectList(_context.Categories, "CategoryID", "Name", course.CategoryID);
        return View(course);
    }

    private bool CourseExists(int id)
    {
        return _context.Courses.Any(e => e.CourseID == id);
    }
    [HttpDelete]
    public async Task<IActionResult> DeleteCourse(int id)
    {
        var course = await _context.Courses
            .Include(c => c.Chapters)
                .ThenInclude(ch => ch.Lessons)
            .FirstOrDefaultAsync(c => c.CourseID == id);

        if (course == null)
        {
            return NotFound();
        }

        foreach (var chapter in course.Chapters)
        {
            _context.Lessons.RemoveRange(chapter.Lessons);
        }

        _context.Chapters.RemoveRange(course.Chapters);

        _context.Courses.Remove(course);

        await _context.SaveChangesAsync();

        return Ok();
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteChapter(int id)
    {
        var chapter = await _context.Chapters.FindAsync(id);
        if (chapter == null)
        {
            return NotFound();
        }

        _context.Chapters.Remove(chapter);
        await _context.SaveChangesAsync();

        return Ok();
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteLesson(int id)
    {
        var lesson = await _context.Lessons.FindAsync(id);
        if (lesson == null)
        {
            return NotFound();
        }

        _context.Lessons.Remove(lesson);
        await _context.SaveChangesAsync();

        return Ok();
    }

}
