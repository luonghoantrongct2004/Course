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
            // Handling course image upload
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

            // Handling chapters, lessons, and questions
            if (chapters != null)
            {
                var chapterList = new List<Chapter>();

                foreach (var chapter in chapters)
                {
                    var newChapter = new Chapter
                    {
                        ChapterID = chapter.ChapterID,
                        Title = chapter.Title,
                        Lessons = new List<Lesson>()
                    };

                    foreach (var lesson in chapter.Lessons)
                    {
                        var videoFileKey = $"Chapters[{chapterList.Count}].Lessons[{newChapter.Lessons.Count}].VideoFile";
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

                        if (lesson.Questions != null)
                        {
                            foreach (var question in lesson.Questions)
                            {
                                question.LessonID = lesson.LessonID;
                                question.Lesson = lesson;

                                if (question.Options != null)
                                {
                                    var optionsList = new List<Option>();
                                    foreach (var option in question.Options)
                                    {
                                        option.QuestionID = question.QuestionID;
                                        option.Question = question;
                                        optionsList.Add(option);
                                    }
                                    question.Options = optionsList;
                                }
                            }
                        }

                        lesson.Chapter = newChapter;
                        newChapter.Lessons.Add(lesson);
                    }

                    chapterList.Add(newChapter);
                }

                course.Chapters = chapterList;
                course.CreatedDate = DateTime.Now;
            }

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Khóa học đã được tạo thành công" });
        }
        return Json(new { success = false, message = "Thông tin nhập vào không hợp lệ. Vui lòng kiểm tra lại." });
    }


    public IActionResult Edit(int id)
    {
        var course = _context.Courses
            .Include(c => c.Chapters)
            .ThenInclude(l => l.Lessons)
            .ThenInclude(q => q.Questions)
            .ThenInclude(o => o.Options)
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
            return Json(new { success = false, message = "Khóa học không tìm thấy!" });
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

                    // Handle chapters, lessons, and questions
                    foreach (var chapter in course.Chapters)
                    {
                        if (chapter.ChapterID != 0)
                        {
                            var existingChapter = await _context.Chapters
                                .Include(c => c.Lessons)
                                    .ThenInclude(l => l.Questions)
                                        .ThenInclude(q => q.Options)
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

                                        // Handle new questions and options
                                        foreach (var question in lesson.Questions)
                                        {
                                            question.LessonID = lesson.LessonID;
                                            _context.Questions.Add(question); // Add new questions to the context

                                            if (question.Options != null)
                                            {
                                                foreach (var option in question.Options)
                                                {
                                                    option.QuestionID = question.QuestionID;
                                                    _context.Options.Add(option); // Add new options to the context
                                                }
                                            }
                                        }

                                        existingChapter.Lessons.Add(lesson);
                                    }
                                    else
                                    {
                                            var existingLesson = await _context.Lessons
                                              .Include(l => l.Questions)
                                                  .ThenInclude(q => q.Options)
                                              .FirstOrDefaultAsync(l => l.LessonID == lesson.LessonID 
                                              && l.ChapterID == chapter.ChapterID);


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

                                            // Handle existing questions and options
                                            foreach (var question in lesson.Questions)
                                            {
                                                if (question.QuestionID == 0)
                                                {
                                                    question.LessonID = lesson.LessonID;
                                                    _context.Questions.Add(question); // Add new questions to the context

                                                    if (question.Options != null)
                                                    {
                                                        foreach (var option in question.Options)
                                                        {
                                                            option.QuestionID = question.QuestionID;
                                                            _context.Options.Add(option); // Add new options to the context
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    var existingQuestion = existingLesson.Questions
                                                        .FirstOrDefault(q => q.QuestionID == question.QuestionID);

                                                    if (existingQuestion != null)
                                                    {
                                                        _context.Entry(existingQuestion).CurrentValues.SetValues(question);

                                                        // Handle options for the question
                                                        foreach (var option in question.Options)
                                                        {
                                                            if (option.OptionID == 0)
                                                            {
                                                                option.QuestionID = question.QuestionID;
                                                                _context.Options.Add(option); // Add new options to the context
                                                            }
                                                            else
                                                            {
                                                                var existingOption = existingQuestion.Options
                                                                    .FirstOrDefault(o => o.OptionID == option.OptionID);

                                                                if (existingOption != null)
                                                                {
                                                                    _context.Entry(existingOption).CurrentValues.SetValues(option);
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }

                                        }
                                    }
                                }

                                await _context.SaveChangesAsync();  // Save changes after updating lessons and questions
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

                                // Handle new questions and options
                                foreach (var question in lesson.Questions)
                                {
                                    question.LessonID = lesson.LessonID;
                                    _context.Questions.Add(question); // Add new questions to the context

                                    if (question.Options != null)
                                    {
                                        foreach (var option in question.Options)
                                        {
                                            option.QuestionID = question.QuestionID;
                                            _context.Options.Add(option); // Add new options to the context
                                        }
                                    }
                                }
                            }

                            _context.Chapters.Add(chapter);
                            await _context.SaveChangesAsync();
                            return Json(new { success = true, message = "Cập nhật khóa học thành công" });
                        }
                    }

                    await transaction.CommitAsync();
                }
                catch (DbUpdateException ex)
                {
                    await transaction.RollbackAsync();
                    ViewData["Categories"] = new SelectList(_context.Categories, "CategoryID", "Name", course.CategoryID);
                    return Json(new { success = false,
                        message = $"Có lỗi trong khi cập nhật: {ex.InnerException?.Message}" });
                }
            }
        }
        return Json(new { success = false, message = "Thông tin nhập vào không hợp lệ. Vui lòng kiểm tra lại." });
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
    [HttpDelete]
    public async Task<IActionResult> DeleteQuestion(int id)
    {
        var question = await _context.Questions.FindAsync(id);
        if (question == null)
        {
            return NotFound();
        }

        _context.Questions.Remove(question);
        await _context.SaveChangesAsync();

        return Ok();
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteOption(int id)
    {
        var option = await _context.Options.FindAsync(id);
        if (option == null)
        {
            return NotFound();
        }

        _context.Options.Remove(option);
        await _context.SaveChangesAsync();

        return Ok();
    }


}
