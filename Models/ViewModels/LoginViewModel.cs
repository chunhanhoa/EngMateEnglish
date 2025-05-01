using System.ComponentModel.DataAnnotations;

namespace TiengAnh.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ")]
        public string Email { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
        
        // Xóa yêu cầu bắt buộc cho ReturnUrl
        public string? ReturnUrl { get; set; }
        
        public bool RememberMe { get; set; }
    }
}
