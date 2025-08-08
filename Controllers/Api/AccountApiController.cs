using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TiengAnh.Models;
using TiengAnh.Repositories;
using TiengAnh.Services;

namespace TiengAnh.Controllers.Api
{
    [ApiController]
    [Route("api/account")]
    [Produces("application/json")]
    public class AccountApiController : ControllerBase
    {
        private readonly ILogger<AccountApiController> _logger;
        private readonly MongoDbService _mongoDbService;
        private readonly UserRepository _userRepository;
        private readonly IWebHostEnvironment _env;

        public AccountApiController(
            ILogger<AccountApiController> logger,
            MongoDbService mongoDbService,
            UserRepository userRepository,
            IWebHostEnvironment env)
        {
            _logger = logger;
            _mongoDbService = mongoDbService;
            _userRepository = userRepository;
            _env = env;
        }

        // GET api/account/me
        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<object>> GetMe()
        {
            var (user, err) = await GetCurrentUserAsync();
            if (user == null) return Unauthorized(new { success = false, message = err ?? "Không xác thực" });

            return Ok(ToDto(user));
        }

        // PUT api/account/profile
        [HttpPut("profile")]
        [Authorize]
        public async Task<ActionResult<object>> UpdateProfile([FromBody] UpdateProfileRequest req)
        {
            var (user, err) = await GetCurrentUserAsync();
            if (user == null) return Unauthorized(new { success = false, message = err ?? "Không xác thực" });

            try
            {
                user.FullName = req.FullName ?? user.FullName;
                user.Phone = req.Phone ?? user.Phone;
                user.Address = req.Address ?? user.Address;
                user.Gender = req.Gender ?? user.Gender;
                user.Bio = req.Bio ?? user.Bio;
                if (!string.IsNullOrWhiteSpace(req.Username))
                {
                    user.Username = req.Username;
                    user.UserName = req.Username;
                }
                user.DateOfBirth = req.DateOfBirth;

                var ok = await _userRepository.UpdateUserAsync(user);
                if (!ok) return StatusCode(500, new { success = false, message = "Không thể cập nhật hồ sơ" });

                return Ok(new { success = true, data = ToDto(user) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update profile failed");
                return StatusCode(500, new { success = false, message = "Lỗi cập nhật hồ sơ" });
            }
        }

        // POST api/account/change-password
        [HttpPost("change-password")]
        [Authorize]
        public async Task<ActionResult<object>> ChangePassword([FromBody] ChangePasswordRequest req)
        {
            if (req is null || string.IsNullOrWhiteSpace(req.CurrentPassword) || string.IsNullOrWhiteSpace(req.NewPassword))
                return BadRequest(new { success = false, message = "Thiếu dữ liệu" });

            var (user, err) = await GetCurrentUserAsync();
            if (user == null) return Unauthorized(new { success = false, message = err ?? "Không xác thực" });

            try
            {
                bool isCurrentValid = false;
                if (user.PasswordHash?.StartsWith("$2") == true)
                    isCurrentValid = BCrypt.Net.BCrypt.Verify(req.CurrentPassword, user.PasswordHash);
                else
                    isCurrentValid = user.PasswordHash == req.CurrentPassword;

                if (!isCurrentValid)
                    return BadRequest(new { success = false, message = "Mật khẩu hiện tại không đúng" });

                string newHash = BCrypt.Net.BCrypt.HashPassword(req.NewPassword);
                var ok = await _userRepository.UpdatePasswordAsync(user.Id ?? user.UserId, newHash);
                if (!ok) return StatusCode(500, new { success = false, message = "Không thể đổi mật khẩu" });

                return Ok(new { success = true, message = "Đổi mật khẩu thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Change password failed");
                return StatusCode(500, new { success = false, message = "Lỗi đổi mật khẩu" });
            }
        }

        // GET api/account/avatar
        [HttpGet("avatar")]
        [Authorize]
        public async Task<ActionResult<object>> GetAvatar()
        {
            var (user, err) = await GetCurrentUserAsync();
            if (user == null) return Unauthorized(new { success = false, message = err ?? "Không xác thực" });

            var avatarPath = string.IsNullOrEmpty(user.Avatar) ? "/images/default-avatar.png" : user.Avatar;
            var ts = DateTime.Now.Ticks;
            return Ok(new
            {
                success = true,
                avatarPath,
                avatarWithTimestamp = $"{avatarPath}{(avatarPath.Contains("?") ? "&" : "?")}v={ts}"
            });
        }

        // POST api/account/avatar
        [HttpPost("avatar")]
        [Authorize]
        public async Task<ActionResult<object>> UpdateAvatar([FromBody] UpdateAvatarRequest req)
        {
            if (req == null || string.IsNullOrEmpty(req.AvatarData))
                return BadRequest(new { success = false, message = "Không có dữ liệu ảnh" });

            var (user, err) = await GetCurrentUserAsync();
            if (user == null) return Unauthorized(new { success = false, message = err ?? "Không xác thực" });

            try
            {
                // Nếu client gửi path trực tiếp
                if (req.AvatarData.StartsWith("/images/avatar/"))
                {
                    string physical = Path.Combine(_env.WebRootPath, req.AvatarData.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (!System.IO.File.Exists(physical))
                        return BadRequest(new { success = false, message = "File ảnh không tồn tại" });

                    user.Avatar = req.AvatarData;
                    var ok = await _userRepository.UpdateUserAsync(user);
                    if (!ok) return StatusCode(500, new { success = false, message = "Lỗi cập nhật avatar" });

                    return Ok(new { success = true, avatarUrl = user.Avatar, message = "Cập nhật ảnh đại diện thành công" });
                }

                // Xử lý base64
                var base64 = req.AvatarData;
                const string prefix = "data:image/";
                var p = base64.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);
                if (p < 0) return BadRequest(new { success = false, message = "Định dạng ảnh không hợp lệ" });

                p += prefix.Length;
                var q = base64.IndexOf(";base64,", p, StringComparison.OrdinalIgnoreCase);
                if (q < 0) return BadRequest(new { success = false, message = "Định dạng dữ liệu không hợp lệ" });

                var ext = base64.Substring(p, q - p).ToLowerInvariant();
                if (!new[] { "jpg", "jpeg", "png", "gif" }.Contains(ext))
                    return BadRequest(new { success = false, message = "Chỉ hỗ trợ JPG, JPEG, PNG, GIF" });

                var dataIndex = base64.IndexOf("base64,", StringComparison.OrdinalIgnoreCase) + "base64,".Length;
                var data = base64.Substring(dataIndex);

                // Thư mục output
                var folder = Path.Combine(_env.WebRootPath, "images", "avatar");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                // Xóa avatar cũ nếu có (không phải default)
                if (!string.IsNullOrEmpty(user.Avatar) && !user.Avatar.Contains("default-avatar"))
                {
                    try
                    {
                        var oldPath = Path.Combine(_env.WebRootPath, user.Avatar.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                        if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                    }
                    catch { /* bỏ qua lỗi xóa */ }
                }

                // Lưu file mới
                var fileName = $"{(user.Id ?? user.UserId ?? Guid.NewGuid().ToString())}_{DateTime.Now:yyyyMMddHHmmss}.{ext}";
                var savePath = Path.Combine(folder, fileName);
                var bytes = Convert.FromBase64String(data);
                await System.IO.File.WriteAllBytesAsync(savePath, bytes);

                var publicPath = $"/images/avatar/{fileName}";
                var updated = await _userRepository.UpdateUserAvatarAsync(user.Id ?? user.UserId, publicPath);
                if (!updated) return StatusCode(500, new { success = false, message = "Lỗi cập nhật CSDL" });

                return Ok(new { success = true, avatarUrl = $"{publicPath}?v={DateTime.Now.Ticks}", message = "Cập nhật ảnh đại diện thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update avatar failed");
                return StatusCode(500, new { success = false, message = "Lỗi xử lý ảnh" });
            }
        }

        // ADMIN: GET api/account/users
        [HttpGet("users")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<object>> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = "")
        {
            var paged = await _userRepository.GetUsersWithPagingAsync(page, pageSize, search ?? string.Empty);
            var dto = new
            {
                paged.CurrentPage,
                paged.TotalPages,
                paged.TotalItems,
                paged.PageSize,
                Items = paged.Items.Select(ToSummaryDto)
            };
            return Ok(new { success = true, data = dto });
        }

        // ADMIN: GET api/account/users/{id}
        [HttpGet("users/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<object>> GetUserById(string id)
        {
            var user = await _userRepository.GetByUserIdAsync(id);
            if (user == null) return NotFound(new { success = false, message = "Không tìm thấy người dùng" });
            return Ok(new { success = true, data = ToDto(user) });
        }

        // ADMIN: POST api/account/users
        [HttpPost("users")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<object>> CreateUser([FromBody] CreateUserRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
                return BadRequest(new { success = false, message = "Thiếu dữ liệu bắt buộc" });

            var exists = await _userRepository.GetByEmailAsync(req.Email);
            if (exists != null) return BadRequest(new { success = false, message = "Email đã được sử dụng" });

            var user = new UserModel
            {
                Email = req.Email,
                Username = req.Username,
                UserName = req.Username,
                FullName = req.FullName ?? req.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
                Avatar = "/images/avatar/default.jpg",
                Role = string.IsNullOrWhiteSpace(req.Role) ? "User" : req.Role,
                Roles = new List<string> { string.IsNullOrWhiteSpace(req.Role) ? "User" : req.Role },
                // Không set Level và Points theo yêu cầu
                CreatedAt = DateTime.Now,
                RegisterDate = DateTime.Now,
                IsVerified = true
            };

            await _mongoDbService.GetCollection<UserModel>("Users").InsertOneAsync(user);
            return CreatedAtAction(nameof(GetUserById), new { id = user.Id ?? user.UserId }, new { success = true, data = ToDto(user) });
        }

        // ADMIN: PUT api/account/users/{id}
        [HttpPut("users/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<object>> UpdateUser(string id, [FromBody] UpdateUserRequest req)
        {
            var user = await _userRepository.GetByUserIdAsync(id);
            if (user == null) return NotFound(new { success = false, message = "Không tìm thấy người dùng" });

            user.FullName = req.FullName ?? user.FullName;
            user.Email = string.IsNullOrWhiteSpace(req.Email) ? user.Email : req.Email;
            user.Username = string.IsNullOrWhiteSpace(req.Username) ? user.Username : req.Username;
            user.UserName = user.Username;
            user.Phone = req.Phone ?? user.Phone;
            user.Address = req.Address ?? user.Address;
            user.Bio = req.Bio ?? user.Bio;
            user.Gender = req.Gender ?? user.Gender;
            user.DateOfBirth = req.DateOfBirth ?? user.DateOfBirth;
            if (!string.IsNullOrWhiteSpace(req.Role))
            {
                user.Role = req.Role;
                user.Roles ??= new List<string>();
                user.Roles.Clear();
                user.Roles.Add(req.Role);
            }
            if (!string.IsNullOrWhiteSpace(req.Level)) user.Level = req.Level;
            if (req.Points.HasValue) user.Points = req.Points.Value;

            var ok = await _userRepository.UpdateUserAsync(user);
            if (!ok) return StatusCode(500, new { success = false, message = "Không thể cập nhật người dùng" });

            return Ok(new { success = true, data = ToDto(user) });
        }

        // ADMIN: DELETE api/account/users/{id}
        [HttpDelete("users/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<object>> DeleteUser(string id)
        {
            var ok = await _userRepository.DeleteUserAsync(id);
            if (!ok) return NotFound(new { success = false, message = "Không thể xóa hoặc không tìm thấy" });
            return NoContent();
        }

        // Helpers
        private async Task<(UserModel? user, string? error)> GetCurrentUserAsync()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = User.FindFirstValue(ClaimTypes.Email);

            if (!string.IsNullOrEmpty(id))
            {
                var byId = await _userRepository.GetByUserIdAsync(id);
                if (byId != null) return (byId, null);
            }
            if (!string.IsNullOrEmpty(email))
            {
                var byEmail = await _userRepository.GetByEmailAsync(email);
                if (byEmail != null) return (byEmail, null);
            }
            return (null, "Không tìm thấy người dùng");
        }

        private static object ToDto(UserModel u) => new
        {
            u.Id,
            u.UserId,
            u.Email,
            Username = u.Username ?? u.UserName,
            u.FullName,
            u.Avatar,
            u.Role,
            u.Roles,
            u.Level,
            u.Points,
            u.Phone,
            u.Gender,
            u.Address,
            u.Bio,
            u.DateOfBirth,
            u.CreatedAt,
            u.RegisterDate,
            u.LastLogin,
            u.IsVerified
        };

        private static object ToSummaryDto(UserModel u) => new
        {
            u.Id,
            u.UserId,
            u.Email,
            Username = u.Username ?? u.UserName,
            u.FullName,
            u.Avatar,
            u.Role,
            u.Level,
            u.RegisterDate,
            u.LastLogin
        };

        // DTOs
        public class UpdateProfileRequest
        {
            public string? FullName { get; set; }
            public string? Phone { get; set; }
            public string? Address { get; set; }
            public string? Gender { get; set; }
            public string? Bio { get; set; }
            public string? Username { get; set; }
            public DateTime? DateOfBirth { get; set; }
        }

        public class ChangePasswordRequest
        {
            public string CurrentPassword { get; set; }
            public string NewPassword { get; set; }
        }

        public class UpdateAvatarRequest
        {
            public string? AvatarData { get; set; }
        }

        public class CreateUserRequest
        {
            public string Email { get; set; }
            public string Username { get; set; }
            public string? FullName { get; set; }
            public string Password { get; set; }
            public string? Role { get; set; }
            // Bỏ Level và Points
        }

        public class UpdateUserRequest
        {
            public string? Email { get; set; }
            public string? Username { get; set; }
            public string? FullName { get; set; }
            public string? Phone { get; set; }
            public string? Address { get; set; }
            public string? Bio { get; set; }
            public string? Gender { get; set; }
            public DateTime? DateOfBirth { get; set; }
            public string? Role { get; set; }
            public string? Level { get; set; }
            public int? Points { get; set; }
        }
    }
}
   