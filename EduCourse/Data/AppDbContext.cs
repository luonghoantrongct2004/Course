using EduCourse.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EduCourse.Data;

public class AppDbContext : IdentityDbContext<User>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var tableName = entityType.GetTableName();
            if (tableName != null && tableName.StartsWith("AspNet"))
            {
                entityType.SetTableName(tableName.Substring(6));
            }
        }
        modelBuilder.Entity<Course>()
        .HasOne(c => c.Category)
        .WithMany(cat => cat.Courses)
        .HasForeignKey(c => c.CategoryID);

        modelBuilder.Entity<Course>()
            .HasOne(c => c.Author)
            .WithMany(u => u.Courses)
            .HasForeignKey(c => c.AuthorID);

        modelBuilder.Entity<ExamQuestion>()
                .HasKey(eq => new { eq.ExamID, eq.QuestionID });

        modelBuilder.Entity<ExamQuestion>()
            .HasOne(eq => eq.Exam)
            .WithMany(e => e.ExamQuestions)
            .HasForeignKey(eq => eq.ExamID);

        modelBuilder.Entity<ExamQuestion>()
            .HasOne(eq => eq.Question)
            .WithMany(q => q.ExamQuestions)
            .HasForeignKey(eq => eq.QuestionID);
        modelBuilder.Entity<Question>()
       .HasOne(q => q.Lesson)
       .WithMany(l => l.Questions)
       .HasForeignKey(q => q.LessonID)
       .IsRequired(false);

        modelBuilder.Entity<Course>()
       .HasOne(c => c.Library) // Course has one Library
       .WithOne(l => l.Course) // Library has one Course
       .HasForeignKey<Library>(l => l.CourseID);
    }

    public DbSet<Course> Courses { get; set; }
    public DbSet<Lesson> Lessons { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<Option> Options { get; set; }
    public DbSet<UserCourse> UserCourses { get; set; }
    public DbSet<StudentQuiz> StudentQuizzes { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Library> Libraries { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Chapter> Chapters { get; set; }
    public DbSet<Exam> Exams { get; set; }
    public DbSet<ExamQuestion> ExamQuestions { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderDetail> OrderDetails { get; set; }
    public DbSet<StudentExam> StudentExams { get; set; }
    public DbSet<StudentExamDetail> StudentExamDetails { get; set; }

}
