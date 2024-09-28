using EduCourse.Data;
using EduCourse.Entities;
using EduCourse.Models;
using EduCourse.SeedDataMigration;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Stripe;

namespace EduCourse
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Cấu hình Identity
            builder.Services.AddIdentity<User, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredUniqueChars = 0;

                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            // Cấu hình Cookie
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Auth/Login";
                options.LogoutPath = "/Auth/Logout";
            });

            // Cấu hình DbContext
            builder.Services.AddDbContext<AppDbContext>(opts =>
            {
                opts.UseSqlServer(builder.Configuration.GetConnectionString("Connection"));
            });
            builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));
            StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Cấu hình request pipeline
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error500");
                app.UseStatusCodePagesWithReExecute("/Home/Error{0}");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication(); // Sử dụng Identity cho Authentication
            app.UseAuthorization();  // Sử dụng Identity cho Authorization

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapControllerRoute(
                 name: "area",
                 pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

            // Khởi tạo roles khi ứng dụng bắt đầu
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    // Khởi tạo các vai trò (Admin, Teacher, Student)
                    await SeedRoles.Initialize(services);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred creating roles");
                }
            }

            await app.RunAsync();
        }
    }
}
