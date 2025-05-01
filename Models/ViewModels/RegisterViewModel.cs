using System.ComponentModel.DataAnnotations;

namespace TiengAnh.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [Display(Name = "Họ tên")]
        public string FullName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ")]
        public string Email { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Tên người dùng là bắt buộc")]
        [Display(Name = "Tên người dùng")]
        public string UserName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [StringLength(100, ErrorMessage = "Mật khẩu phải có ít nhất {2} ký tự và không quá {1} ký tự", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc")]
        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu")]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp với mật khẩu đã nhập")]
        public string ConfirmPassword { get; set; } = string.Empty;

        // Thêm thuộc tính AcceptTerms
        [Required(ErrorMessage = "Bạn phải đồng ý với điều khoản sử dụng")]
        public bool AcceptTerms { get; set; }

        // Thêm phương thức khởi tạo mặc định để khởi tạo các thuộc tính
        public RegisterViewModel()
        {
            FullName = string.Empty;
            Email = string.Empty;
            UserName = string.Empty;
            Password = string.Empty;
            ConfirmPassword = string.Empty;
        }
    }
}
