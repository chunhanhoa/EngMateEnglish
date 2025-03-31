using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using TiengAnh.Data;
using TiengAnh.Models;
using System.IO;

namespace TiengAnh.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly HocTiengAnhContext _context;

        public AccountController(ILogger<AccountController> logger, HocTiengAnhContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            // Xử lý khi returnUrl là null hoặc rỗng
            if (string.IsNullOrEmpty(model.ReturnUrl) || model.ReturnUrl == "/")
            {
                model.ReturnUrl = "/";
            }

            ViewData["ReturnUrl"] = model.ReturnUrl;

            if (ModelState.IsValid)
            {
                // Log thông tin để debug
                _logger.LogInformation("Đang thử đăng nhập với Email/Username: {Email}", model.Email);

                // Tìm kiếm người dùng trong cơ sở dữ liệu dựa trên email hoặc tên đăng nhập
                var user = await _context.TaiKhoans
                    .Include(t => t.IdQNavigation) // Include thông tin quyền
                    .FirstOrDefaultAsync(u => (u.EmailTk == model.Email || u.NameTk == model.Email) && u.PasswordTk == model.Password);

                if (user != null)
                {
                    _logger.LogInformation("Người dùng đăng nhập thành công: {Email}", model.Email);

                    // Lấy đường dẫn avatar hoặc mặc định nếu không có
                    string avatarPath = string.IsNullOrEmpty(user.AvatarTk) ? "/images/avatars/default.jpg" : user.AvatarTk;

                    // Tạo các claim cho người dùng
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.DisplaynameTk ?? user.NameTk ?? string.Empty),
                        new Claim(ClaimTypes.NameIdentifier, user.IdTk.ToString()),
                        new Claim(ClaimTypes.Email, user.EmailTk ?? string.Empty),
                        new Claim(ClaimTypes.Role, user.IdQNavigation?.NameQ ?? "student"),
                        new Claim("DisplayName", user.DisplaynameTk ?? string.Empty),
                        new Claim("UserID", user.IdTk.ToString()),
                        new Claim("UserRole", user.IdQNavigation?.NameQ ?? "student"),
                        new Claim("Avatar", avatarPath) // Thêm avatar vào claims
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                    };

                    // Đăng nhập người dùng
                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    // Chuyển hướng người dùng
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    return RedirectToAction("Index", "Home");
                }

                // Ghi log khi đăng nhập thất bại
                _logger.LogWarning("Đăng nhập thất bại cho email/username: {Email}", model.Email);
                ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không đúng.");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra xem email đã tồn tại chưa
                var existingUser = await _context.TaiKhoans
                    .FirstOrDefaultAsync(u => u.EmailTk == model.Email);

                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Email này đã được sử dụng.");
                    return View(model);
                }

                // Tạo tài khoản mới (quyền mặc định là 3 - Student)
                var newUser = new TaiKhoan
                {
                    NameTk = model.Email.Split('@')[0], // Tạo username từ phần đầu của email
                    EmailTk = model.Email,
                    PasswordTk = model.Password, // Lưu ý: Trong thực tế, bạn nên mã hóa mật khẩu
                    DisplaynameTk = model.FullName,
                    IdQ = 3 // Student role
                };

                try
                {
                    _context.TaiKhoans.Add(newUser);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Người dùng đã tạo tài khoản mới: {Email}", model.Email);

                    // Tự động đăng nhập sau khi đăng ký
                    await LoginAfterRegistration(newUser);

                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi đăng ký người dùng mới");
                    ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi đăng ký. Vui lòng thử lại sau.");
                }
            }

            return View(model);
        }

        private async Task LoginAfterRegistration(TaiKhoan user)
        {
            // Lấy đường dẫn avatar mặc định cho người dùng mới
            string avatarPath = "/images/avatars/default.jpg";

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.DisplaynameTk ?? user.NameTk ?? string.Empty),
                new Claim(ClaimTypes.NameIdentifier, user.IdTk.ToString()),
                new Claim(ClaimTypes.Email, user.EmailTk ?? string.Empty),
                new Claim(ClaimTypes.Role, "student"), // Quyền mặc định cho người dùng mới
                new Claim("DisplayName", user.DisplaynameTk ?? string.Empty),
                new Claim("UserID", user.IdTk.ToString()),
                new Claim("UserRole", "student"),
                new Claim("Avatar", avatarPath) // Thêm avatar vào claims
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            // Kiểm tra xem người dùng đã đăng nhập chưa
            if (!(User.Identity?.IsAuthenticated ?? false))
            {
                return RedirectToAction("Login");
            }

            // Lấy ID người dùng từ claims
            var userIdStr = User.FindFirstValue("UserID");

            if (!int.TryParse(userIdStr, out int userId))
            {
                return NotFound();
            }

            // Lấy thông tin người dùng từ database
            var dbUser = await _context.TaiKhoans
                .Include(t => t.IdQNavigation)
                .FirstOrDefaultAsync(u => u.IdTk == userId);

            if (dbUser == null)
            {
                return NotFound();
            }

            // Lấy thông tin học tập từ database (ví dụ: số từ vựng đã học, số bài tập đã làm)
            var learningStats = await GetLearningStatisticsAsync(userId);

            // Lấy hoạt động gần đây
            var recentActivities = await GetRecentActivitiesAsync(userId);

            // Chuyển đổi thành UserModel để hiển thị
            var userModel = new UserModel
            {
                Id = dbUser.IdTk.ToString(),
                UserName = dbUser.NameTk,
                Email = dbUser.EmailTk,
                FullName = dbUser.DisplaynameTk,
                PhoneNumber = dbUser.PhoneTk,
                Avatar = string.IsNullOrEmpty(dbUser.AvatarTk) ? "/images/avatars/default.jpg" : dbUser.AvatarTk,
                Level = await CalculateUserLevelAsync(userId),
                RegisterDate = DateTime.Now, // Thời gian đăng ký thật nếu có trong DB
                Points = await CalculateUserPointsAsync(userId),
                Roles = new List<string> { dbUser.IdQNavigation?.NameQ ?? "student" },
                VocabularyProgress = learningStats.VocabularyProgress,
                GrammarProgress = learningStats.GrammarProgress,
                ExerciseProgress = learningStats.ExerciseProgress,
                RecentActivities = recentActivities
            };

            return View(userModel);
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            // Kiểm tra xem người dùng đã đăng nhập chưa
            if (!(User.Identity?.IsAuthenticated ?? false))
            {
                return RedirectToAction("Login");
            }

            // Lấy ID người dùng từ claims
            var userIdStr = User.FindFirstValue("UserID");

            if (!int.TryParse(userIdStr, out int userId))
            {
                return NotFound();
            }

            // Lấy thông tin người dùng từ database
            var dbUser = await _context.TaiKhoans
                .FirstOrDefaultAsync(u => u.IdTk == userId);

            if (dbUser == null)
            {
                return NotFound();
            }

            // Chuyển đổi thành EditProfileViewModel để hiển thị form chỉnh sửa
            var model = new EditProfileViewModel
            {
                FullName = dbUser.DisplaynameTk,
                Email = dbUser.EmailTk,
                PhoneNumber = dbUser.PhoneTk,
                UserName = dbUser.NameTk
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Lấy ID người dùng từ claims
            var userIdStr = User.FindFirstValue("UserID");

            if (!int.TryParse(userIdStr, out int userId))
            {
                return NotFound();
            }

            // Lấy thông tin người dùng từ database
            var dbUser = await _context.TaiKhoans
                .FirstOrDefaultAsync(u => u.IdTk == userId);

            if (dbUser == null)
            {
                return NotFound();
            }

            // Cập nhật thông tin
            dbUser.DisplaynameTk = model.FullName;
            dbUser.PhoneTk = model.PhoneNumber ?? string.Empty;

            // Kiểm tra nếu email được thay đổi và đảm bảo không trùng với người dùng khác
            if (dbUser.EmailTk != model.Email)
            {
                var emailExists = await _context.TaiKhoans
                    .AnyAsync(u => u.EmailTk == model.Email && u.IdTk != userId);

                if (emailExists)
                {
                    ModelState.AddModelError("Email", "Email này đã được sử dụng bởi tài khoản khác.");
                    return View(model);
                }

                dbUser.EmailTk = model.Email;
            }

            // Kiểm tra nếu mật khẩu được cung cấp
            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                if (dbUser.PasswordTk != model.CurrentPassword)
                {
                    ModelState.AddModelError("CurrentPassword", "Mật khẩu hiện tại không đúng.");
                    return View(model);
                }

                dbUser.PasswordTk = model.NewPassword;
            }

            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thông tin cá nhân đã được cập nhật thành công.";
                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật thông tin người dùng");
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi cập nhật thông tin. Vui lòng thử lại sau.");
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> UploadAvatar(IFormFile avatar)
        {
            // Kiểm tra xem người dùng đã đăng nhập chưa
            if (!User.Identity?.IsAuthenticated ?? false)
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập để thực hiện hành động này." });
            }

            // Kiểm tra tập tin tải lên
            if (avatar == null || avatar.Length == 0)
            {
                return Json(new { success = false, message = "Không tìm thấy tập tin tải lên." });
            }

            // Kiểm tra định dạng tập tin
            if (!IsValidImageFile(avatar))
            {
                return Json(new { success = false, message = "Chỉ chấp nhận hình ảnh định dạng JPG, PNG, GIF." });
            }

            // Lấy ID người dùng từ claims
            var userIdStr = User.FindFirstValue("UserID");
            
            if (!int.TryParse(userIdStr, out int userId))
            {
                return Json(new { success = false, message = "Không xác định được người dùng." });
            }

            try
            {
                // Lấy thông tin người dùng từ database
                var user = await _context.TaiKhoans.FindAsync(userId);
                
                if (user == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin người dùng." });
                }

                // Tạo thư mục lưu ảnh nếu chưa tồn tại
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "avatars");
                
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Tạo tên file duy nhất
                string uniqueFileName = $"{userId}_{Guid.NewGuid().ToString().Substring(0, 8)}{Path.GetExtension(avatar.FileName)}";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Lưu tập tin
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await avatar.CopyToAsync(stream);
                }

                // Xóa ảnh cũ (nếu có và không phải ảnh mặc định)
                if (!string.IsNullOrEmpty(user.AvatarTk) && 
                    !user.AvatarTk.EndsWith("default.jpg") && 
                    System.IO.File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.AvatarTk.TrimStart('/'))))
                {
                    System.IO.File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.AvatarTk.TrimStart('/')));
                }

                // Cập nhật đường dẫn ảnh trong database
                string relativePath = $"/images/avatars/{uniqueFileName}";
                user.AvatarTk = relativePath;
                await _context.SaveChangesAsync();

                // Cập nhật claim Avatar
                var identity = User.Identity as ClaimsIdentity;
                if (identity != null)
                {
                    var existingClaim = identity.FindFirst("Avatar");
                    if (existingClaim != null)
                    {
                        identity.RemoveClaim(existingClaim);
                    }
                    identity.AddClaim(new Claim("Avatar", relativePath));
                    
                    // Cập nhật principal
                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(identity));
                }

                return Json(new { success = true, message = "Cập nhật ảnh đại diện thành công.", avatarUrl = relativePath });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật ảnh đại diện cho người dùng {UserId}", userId);
                return Json(new { success = false, message = "Đã xảy ra lỗi khi cập nhật ảnh đại diện. Vui lòng thử lại sau." });
            }
        }

        private bool IsValidImageFile(IFormFile file)
        {
            // Kiểm tra định dạng tập tin
            string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
            string fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            
            if (string.IsNullOrEmpty(fileExtension) || !allowedExtensions.Contains(fileExtension))
            {
                return false;
            }

            // Kiểm tra MIME type
            string[] allowedMimeTypes = { "image/jpeg", "image/png", "image/gif" };
            return allowedMimeTypes.Contains(file.ContentType);
        }

        // Phương thức hỗ trợ để lấy số liệu thống kê học tập
        private async Task<(int VocabularyProgress, int GrammarProgress, int ExerciseProgress)> GetLearningStatisticsAsync(int userId)
        {
            // Đếm số từ vựng đã học
            var vocabularyLearned = await _context.TienTrinhHocs
                .CountAsync(t => t.IdTk == userId && t.TypeTth == "TuVung");

            // Đếm số ngữ pháp đã học
            var grammarLearned = await _context.TienTrinhHocs
                .CountAsync(t => t.IdTk == userId && t.TypeTth == "NguPhap");

            // Đếm số bài tập đã làm
            var exerciseCompleted = await _context.TienTrinhHocs
                .CountAsync(t => t.IdTk == userId && t.TypeTth == "BaiTap");

            // Tổng số từ vựng, ngữ pháp, bài tập trong hệ thống
            var totalVocabulary = await _context.TuVungs.CountAsync();
            var totalGrammar = await _context.NguPhaps.CountAsync();
            var totalExercises = await _context.BaiTaps.CountAsync();

            // Tính phần trăm
            int vocabularyProgress = totalVocabulary > 0 ? (vocabularyLearned * 100) / totalVocabulary : 0;
            int grammarProgress = totalGrammar > 0 ? (grammarLearned * 100) / totalGrammar : 0;
            int exerciseProgress = totalExercises > 0 ? (exerciseCompleted * 100) / totalExercises : 0;

            return (vocabularyProgress, grammarProgress, exerciseProgress);
        }

        // Phương thức hỗ trợ để lấy hoạt động gần đây
        private async Task<List<UserActivity>> GetRecentActivitiesAsync(int userId)
        {
            var activities = await _context.TienTrinhHocs
                .Where(t => t.IdTk == userId)
                .OrderByDescending(t => t.LastTimeStudyTth)
                .Take(5)
                .ToListAsync();

            var result = new List<UserActivity>();

            foreach (var activity in activities)
            {
                string title = "";
                string description = "";
                string type = activity.TypeTth ?? "Unknown";
                DateTime timestamp = activity.LastTimeStudyTth ?? DateTime.Now;

                if (type == "TuVung")
                {
                    var vocab = await _context.TuVungs
                        .Include(t => t.IdCdNavigation)
                        .FirstOrDefaultAsync(t => t.IdTv == activity.IdTypeTth);

                    if (vocab != null)
                    {
                        title = "Học từ vựng";
                        description = $"Đã học từ \"{vocab?.WordTv ?? "không xác định"}\" trong chủ đề {vocab?.IdCdNavigation?.NameCd ?? "không xác định"}";
                    }
                }
                else if (type == "NguPhap")
                {
                    var grammar = await _context.NguPhaps
                        .FirstOrDefaultAsync(n => n.IdNp == activity.IdTypeTth);

                    if (grammar != null)
                    {
                        title = "Học ngữ pháp";
                        description = $"Đã học \"{grammar.TitleNp}\"";
                    }
                }
                else if (type == "BaiTap")
                {
                    title = "Làm bài tập";
                    description = "Đã hoàn thành một bài tập";
                }

                result.Add(new UserActivity
                {
                    Title = title,
                    Description = description,
                    Type = type,
                    Timestamp = timestamp
                });
            }

            return result;
        }

        private async Task<string> CalculateUserLevelAsync(int userId)
        {
            // Tính điểm người dùng
            var points = await CalculateUserPointsAsync(userId);

            // Xác định cấp độ dựa trên điểm
            if (points < 100) return "A1";
            if (points < 300) return "A2";
            if (points < 600) return "B1";
            if (points < 1000) return "B2";
            if (points < 1500) return "C1";
            return "C2";
        }

        private async Task<int> CalculateUserPointsAsync(int userId)
        {
            // Đếm số từ vựng đã học
            var vocabularyLearned = await _context.TienTrinhHocs
                .CountAsync(t => t.IdTk == userId && t.TypeTth == "TuVung");

            // Đếm số ngữ pháp đã học
            var grammarLearned = await _context.TienTrinhHocs
                .CountAsync(t => t.IdTk == userId && t.TypeTth == "NguPhap");

            // Đếm số bài tập đã làm
            var exerciseCompleted = await _context.TienTrinhHocs
                .CountAsync(t => t.IdTk == userId && t.TypeTth == "BaiTap");

            // Tính điểm: mỗi từ vựng 5 điểm, mỗi ngữ pháp 10 điểm, mỗi bài tập 3 điểm
            return (vocabularyLearned * 5) + (grammarLearned * 10) + (exerciseCompleted * 3);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        // Tạo action này để debug - khi cần kiểm tra thông tin đăng nhập
        [HttpGet]
        public IActionResult LoginStatus()
        {
            var isAuthenticated = User.Identity?.IsAuthenticated ?? false;
            var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();

            return Json(new
            {
                IsAuthenticated = isAuthenticated,
                Username = User.Identity?.Name ?? string.Empty,
                Claims = claims,
                DatabaseName = _context.Database.GetDbConnection().Database,
                DatabaseState = _context.Database.CanConnect() ? "Connected" : "Not Connected"
            });
        }

        // Add this action to your AccountController class
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
