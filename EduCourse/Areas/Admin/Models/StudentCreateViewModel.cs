using System.ComponentModel.DataAnnotations;

namespace EduCourse.Areas.Admin.Models;

public class StudentCreateViewModel
{
    [Required(ErrorMessage = "Họ và tên là bắt buộc")]
    public string FullName { get; set; }

    [Required(ErrorMessage = "Email là bắt buộc")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    public string PhoneNumber { get; set; }

    [Required(ErrorMessage = "Địa chỉ là bắt buộc")]
    public string Address { get; set; }

    [Required(ErrorMessage = "Trạng thái là bắt buộc")]
    public string Status { get; set; }

    [Required(ErrorMessage = "Ảnh là bắt buộc")]
    public IFormFile? Photo { get; set; }
}
