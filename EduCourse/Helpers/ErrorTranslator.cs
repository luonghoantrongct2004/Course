namespace EduCourse.Helpers;

public class ErrorTranslator
{
    public static string Translate(string englishError)
    {
        // Define the dictionary with translations
        var translations = new Dictionary<string, string>
    {
        // Password related errors
        { "Passwords must have at least one digit ('0'-'9').", "Mật khẩu phải chứa ít nhất một chữ số ('0'-'9')." },
        { "Passwords must have at least one lowercase ('a'-'z').", "Mật khẩu phải chứa ít nhất một chữ thường ('a'-'z')." },
        { "Passwords must have at least one uppercase ('A'-'Z').", "Mật khẩu phải chứa ít nhất một chữ hoa ('A'-'Z')." },
        { "Passwords must have at least one non-alphanumeric character.", "Mật khẩu phải chứa ít nhất một ký tự đặc biệt." },
        { "Passwords must be at least 6 characters.", "Mật khẩu phải có ít nhất 6 ký tự." },
        { "Passwords must be at least 8 characters.", "Mật khẩu phải có ít nhất 8 ký tự." },
        { "Password mismatch.", "Mật khẩu không khớp." },

        // User and email related errors
        { "User name is already taken.", "Tên người dùng đã được sử dụng." },
        { "Email is already taken.", "Địa chỉ email đã được sử dụng." },
        { "The email is already taken.", "Địa chỉ email đã được sử dụng." },
        { "Invalid token.", "Mã token không hợp lệ." },
        { "Invalid email address.", "Địa chỉ email không hợp lệ." },
        { "Email is required.", "Địa chỉ email là bắt buộc." },
        { "Email cannot be empty.", "Địa chỉ email không được để trống." },

        // Login related errors
        { "Invalid login attempt.", "Thử đăng nhập không thành công." },
        { "Invalid user name or password.", "Tên đăng nhập hoặc mật khẩu không đúng." },
        { "Email not confirmed.", "Địa chỉ email chưa được xác nhận." },
        { "Lockout is not enabled for this user.", "Tài khoản này không bị khóa." },
        { "The user is locked out.", "Tài khoản này đã bị khóa." },
        { "The user is already in the role.", "Người dùng đã có vai trò này." },
        { "The user does not exist.", "Người dùng không tồn tại." },

        // Role related errors
        { "Role name is already taken.", "Tên vai trò đã được sử dụng." },
        { "Role does not exist.", "Vai trò không tồn tại." },

        // Token errors
        { "Invalid token.", "Mã token không hợp lệ." },
        { "Recovery code is invalid.", "Mã khôi phục không hợp lệ." },
        { "The token is expired.", "Mã token đã hết hạn." },

        // Default fallback for unknown errors
        { "An unknown error occurred.", "Đã xảy ra lỗi không xác định." }
    };

        // Check if the dictionary contains the error and return the translation, or return the original error
        if (translations.ContainsKey(englishError))
        {
            return translations[englishError];
        }

        // Fallback: return the original message if no translation is found
        return "Lỗi không xác định: " + englishError;
    }

}
