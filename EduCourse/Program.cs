using EduCourse.Data;
using EduCourse.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EduCourse
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddIdentity<User, Role>(options =>
            {
                options.Password.RequiredLength = 8; // Đặt độ dài tối thiểu là 8 ký tự
                options.Password.RequireDigit = false; // Không yêu cầu chữ số
                options.Password.RequireLowercase = false; // Không yêu cầu chữ cái thường
                options.Password.RequireUppercase = false; // Không yêu cầu chữ cái hoa
                options.Password.RequireNonAlphanumeric = false; // Không yêu cầu ký tự đặc biệt
                options.Password.RequiredUniqueChars = 0; // Không yêu cầu các ký tự độc nhất

                // Cấu hình khóa bảo mật
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // Cấu hình xác thực
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();


            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Auth/Login";
                options.LogoutPath = "/Auth/Logout";
            });
            builder.Services.AddControllersWithViews();

            builder.Services.AddDbContext<AppDbContext>(opts =>
            {
                opts.UseSqlServer(builder.Configuration.GetConnectionString("Connection"));
            });
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error500");
                app.UseStatusCodePagesWithReExecute("/Home/Error{0}");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapControllerRoute(
                 name: "area",
                 pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");


            app.Run();
        }
    }
}
