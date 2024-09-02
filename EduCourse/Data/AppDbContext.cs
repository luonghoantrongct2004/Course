using EduCourse.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EduCourse.Data;

public class AppDbContext : IdentityDbContext<User, Role, int>
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
    }

    public DbSet<Course> Courses { get; set; }
    public DbSet<Lesson> Lessons { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<Option> Options { get; set; }
    public DbSet<UserCourse> UserCourses { get; set; }
    public DbSet<StudentQuiz> StudentQuizzes { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<EduCourse.Entities.File> Files { get; set; }
    public DbSet<Library> Libraries { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Chapter> Chapters { get; set; }
}
