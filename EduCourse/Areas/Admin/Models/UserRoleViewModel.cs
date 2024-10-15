using EduCourse.Entities;
using Microsoft.AspNetCore.Identity;

namespace EduCourse.Areas.Admin.Models;

public class UserRoleViewModel
{
    public List<User> Users { get; set; }
    public List<IdentityRole> Roles { get; set; }
    public Dictionary<string, List<string>> UserRoles { get; set; }
}

