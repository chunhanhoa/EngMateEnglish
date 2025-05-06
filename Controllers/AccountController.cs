using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TiengAnh.Models;
using TiengAnh.Models.ViewModels;
using TiengAnh.Repositories;
using TiengAnh.Services;
using BCrypt.Net;

namespace TiengAnh.Controllers
{
    public class AccountController : BaseController
    {
        private readonly ILogger<AccountController> _logger;
        private readonly MongoDbService _mongoDbService;
        private readonly UserRepository _userRepository;
        private readonly string _webRootPath;

        public AccountController(
            ILogger<AccountController> logger,
            MongoDbService mongoDbService,
            UserRepository userRepository,
            IWebHostEnvironment environment) : base(mongoDbService)
        {
            _logger = logger;
            _mongoDbService = mongoDbService;
            _userRepository = userRepository;
            _webRootPath = environment.WebRootPath;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Profile", "Account");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(new Models.ViewModels.LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        public async Task<IActionResult> Login(TiengAnh.Models.ViewModels.LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _mongoDbService.GetCollection<UserModel>("Users")
                        .Find(u => u.Email.ToLower() == model.Email.ToLower())
                        .FirstOrDefaultAsync();

                    if (user != null)
                    {
                        bool isPasswordValid = false;
                        try
                        {
                            if (user.PasswordHash?.StartsWith("$2") == true)
                            {
                                isPasswordValid = BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash);
                            }
                            else
                            {
                                isPasswordValid = (user.PasswordHash == model.Password);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Password verification error: {ex.Message}");
                            isPasswordValid = (user.PasswordHash == model.Password);
                        }

                        if (isPasswordValid)
                        {
                            var claims = new List<Claim>
                            {
                                new Claim(ClaimTypes.NameIdentifier, user.Id ?? user.UserId ?? Guid.NewGuid().ToString()),
                                new Claim(ClaimTypes.Name, user.UserName ?? user.Username ?? "unknown"),
                                new Claim(ClaimTypes.Email, user.Email),
                                new Claim(ClaimTypes.Role, user.Role ?? (user.Roles?.FirstOrDefault() ?? "User"))
                            };

                            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                            var authProperties = new AuthenticationProperties
                            {
                                IsPersistent = model.RememberMe,
                                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                            };

                            await HttpContext.SignInAsync(
                                CookieAuthenticationDefaults.AuthenticationScheme,
                                new ClaimsPrincipal(claimsIdentity),
                                authProperties);
                            
                            _logger.LogInformation($"User {user.Email} logged in successfully");
                            return RedirectToAction("Profile", "Account");
                        }
                    }

                    ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng.");
                    return View(model);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Login error: {ex.Message}");
                    ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi trong quá trình đăng nhập.");
                    return View(model);
                }
            }

            return View(model);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            string userEmail = User.FindFirstValue(ClaimTypes.Email);
            
            _logger.LogInformation($"Profile: Loading for user ID: {userId}, Email: {userEmail}");
            
            var user = await _userRepository.GetByUserIdAsync(userId);
            if (user == null && !string.IsNullOrEmpty(userEmail))
            {
                user = await _userRepository.GetByEmailAsync(userEmail);
            }

            if (user == null)
            {
                _logger.LogWarning($"Profile: User not found for ID: {userId}, Email: {userEmail}");
                TempData["ErrorMessage"] = "Không tìm thấy thông tin người dùng.";
                return RedirectToAction("Index", "Home");
            }
            
            // Ensure the avatar URL includes cache busting
            if (!string.IsNullOrEmpty(user.Avatar))
            {
                user.Avatar = user.Avatar + (user.Avatar.Contains("?") ? "&" : "?") + $"v={DateTime.Now.Ticks}";
            }
            
            _logger.LogInformation($"Profile: Loaded for user ID: {user.Id}, Avatar: {user.Avatar}");
            
            return View(user);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(TiengAnh.Models.ViewModels.RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _mongoDbService.GetCollection<UserModel>("Users")
                    .Find(u => u.Email.ToLower() == model.Email.ToLower())
                    .FirstOrDefaultAsync();
                if (existingUser != null)
                {
                    ModelState.AddModelError(string.Empty, "Email đã được sử dụng.");
                    return View(model);
                }

                var user = new UserModel
                {
                    Email = model.Email,
                    Username = model.UserName,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    FullName = model.FullName,
                    Avatar = "/images/avatar/default.jpg",
                    Role = "User",
                    Roles = new List<string> { "User" },
                    CreatedAt = DateTime.Now,
                    RegisterDate = DateTime.Now,
                    IsVerified = false
                };

                await _mongoDbService.GetCollection<UserModel>("Users").InsertOneAsync(user);
                return RedirectToAction("Login");
            }

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(UserModel model)
        {
            try
            {
                _logger.LogInformation($"UpdateProfile: ID={model.Id}, FullName={model.FullName}, Avatar={model.Avatar}");
                _logger.LogInformation($"UpdateProfile: DateOfBirth from form: {model.DateOfBirth}");
                
                string userIdFromClaims = User.FindFirstValue(ClaimTypes.NameIdentifier);
                string emailFromClaims = User.FindFirstValue(ClaimTypes.Email);
                
                var currentUser = await _userRepository.GetByUserIdAsync(userIdFromClaims);
                if (currentUser == null && !string.IsNullOrEmpty(emailFromClaims))
                {
                    currentUser = await _userRepository.GetByEmailAsync(emailFromClaims);
                }

                if (currentUser == null)
                {
                    _logger.LogWarning($"UpdateProfile: User not found for ID: {userIdFromClaims}, Email: {emailFromClaims}");
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin người dùng hiện tại.";
                    return View("Profile", model);
                }

                _logger.LogInformation($"UpdateProfile: Found user: ID={currentUser.Id}, Email={currentUser.Email}, Avatar={currentUser.Avatar}");
                _logger.LogInformation($"UpdateProfile: Current DateOfBirth: {currentUser.DateOfBirth}");

                currentUser.FullName = model.FullName;
                currentUser.Phone = model.Phone ?? "";
                currentUser.Address = model.Address ?? "";
                currentUser.Gender = model.Gender ?? "";
                currentUser.Bio = model.Bio ?? "";
                currentUser.Username = model.Username ?? currentUser.Username;
                currentUser.UserName = model.Username ?? currentUser.UserName;
                
                if (model.DateOfBirth.HasValue)
                {
                    var day = model.DateOfBirth.Value.Day;
                    var month = model.DateOfBirth.Value.Month;
                    var year = model.DateOfBirth.Value.Year;
                    currentUser.DateOfBirth = new DateTime(year, month, day, 12, 0, 0, DateTimeKind.Local);
                    _logger.LogInformation($"UpdateProfile: Set DateOfBirth: {currentUser.DateOfBirth}, Day={day}, Month={month}, Year={year}");
                }
                else
                {
                    currentUser.DateOfBirth = null;
                }
                
                if (!string.IsNullOrEmpty(model.Avatar))
                {
                    currentUser.Avatar = model.Avatar;
                }
                
                bool updateResult = await _userRepository.UpdateUserAsync(currentUser);
                _logger.LogInformation($"UpdateProfile: Update result: {updateResult}");
                
                if (updateResult)
                {
                    var updatedUser = await _userRepository.GetByUserIdAsync(currentUser.Id);
                    _logger.LogInformation($"UpdateProfile: After update - DateOfBirth: {updatedUser?.DateOfBirth}, Avatar: {updatedUser?.Avatar}");
                    
                    TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
                    return RedirectToAction("Profile", new { v = DateTime.Now.Ticks });
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể cập nhật thông tin người dùng.";
                }

                return View("Profile", currentUser);
            }
            catch (Exception ex)
            {
                _logger.LogError($"UpdateProfile: Error: {ex.Message}, Stack: {ex.StackTrace}");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi cập nhật thông tin: " + ex.Message;
                return View("Profile", model);
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAvatarUrl()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                _logger.LogInformation($"GetAvatarUrl: User ID from Claims: {userId}");

                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("GetAvatarUrl: Người dùng chưa đăng nhập");
                    return Json(new { success = false, message = "Người dùng chưa đăng nhập" });
                }

                var user = await _userRepository.GetByUserIdAsync(userId);
                if (user == null)
                {
                    var email = User.FindFirstValue(ClaimTypes.Email);
                    if (!string.IsNullOrEmpty(email))
                    {
                        user = await _userRepository.GetByEmailAsync(email);
                        _logger.LogInformation($"GetAvatarUrl: Found user by email: {email}, User: {user?.Id}, Avatar: {user?.Avatar}");
                    }
                }
                else
                {
                    _logger.LogInformation($"GetAvatarUrl: Found user by userId: {userId}, User: {user?.Id}, Avatar: {user?.Avatar}");
                }

                if (user == null)
                {
                    _logger.LogWarning($"GetAvatarUrl: User not found for ID: {userId}");
                    return Json(new { success = false, message = "Không tìm thấy người dùng" });
                }

                var avatarPath = !string.IsNullOrEmpty(user.Avatar) ? user.Avatar : "/images/default-avatar.png";
                var timestamp = DateTime.Now.Ticks;
                var avatarWithTimestamp = $"{avatarPath}?v={timestamp}";

                Response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
                Response.Headers.Add("Pragma", "no-cache");
                Response.Headers.Add("Expires", "0");

                return Json(new { success = true, avatarPath, avatarWithTimestamp });
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetAvatarUrl: Error: {ex.Message}, StackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = "Lỗi khi lấy đường dẫn avatar" });
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAvatar([FromBody] AvatarUpdateModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.AvatarData))
                {
                    _logger.LogWarning("UpdateAvatar: No avatar data provided");
                    return Json(new { success = false, message = "Không có dữ liệu ảnh" });
                }

                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                string userEmail = User.FindFirstValue(ClaimTypes.Email);
                
                _logger.LogInformation($"UpdateAvatar: Processing for user ID: {userId}, Email: {userEmail}");
                
                var user = await _userRepository.GetByUserIdAsync(userId);
                if (user == null && !string.IsNullOrEmpty(userEmail))
                {
                    user = await _userRepository.GetByEmailAsync(userEmail);
                }

                if (user == null)
                {
                    _logger.LogWarning($"UpdateAvatar: User not found for ID: {userId}, Email: {userEmail}");
                    return Json(new { success = false, message = "Không thể tìm thấy thông tin người dùng" });
                }

                _logger.LogInformation($"UpdateAvatar: Found user: ID={user.Id}, Email={user.Email}, Current Avatar={user.Avatar}");

                // Handle direct path (from existing avatar)
                if (model.AvatarData.StartsWith("/images/avatar/"))
                {
                    _logger.LogInformation($"UpdateAvatar: Using direct path: {model.AvatarData}");
                    
                    // Make sure the file exists
                    string physicalPath = Path.Combine(_webRootPath, model.AvatarData.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (!System.IO.File.Exists(physicalPath))
                    {
                        _logger.LogWarning($"UpdateAvatar: Avatar file does not exist at {physicalPath}");
                        return Json(new { success = false, message = "File ảnh không tồn tại" });
                    }
                    
                    user.Avatar = model.AvatarData;
                    var updateResult = await _userRepository.UpdateUserAsync(user);
                    
                    if (updateResult)
                    {
                        _logger.LogInformation($"UpdateAvatar: Updated avatar to {user.Avatar} for user ID: {user.Id}");
                        
                        // Set cache control headers
                        Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
                        Response.Headers["Pragma"] = "no-cache";
                        Response.Headers["Expires"] = "0";
                        
                        return Json(new { 
                            success = true, 
                            avatarUrl = user.Avatar,
                            message = "Cập nhật ảnh đại diện thành công!",
                            reload = true
                        });
                    }
                    else
                    {
                        _logger.LogWarning($"UpdateAvatar: Failed to update avatar in database for user ID: {user.Id}");
                        return Json(new { success = false, message = "Lỗi cập nhật vào database" });
                    }
                }
                else
                {
                    try 
                    {
                        // Handle base64 data
                        string base64Data = model.AvatarData;
                        string dataPrefix = "data:image/";
                        
                        int startIndex = base64Data.IndexOf(dataPrefix);
                        if (startIndex < 0)
                        {
                            _logger.LogWarning("UpdateAvatar: Invalid image format");
                            return Json(new { success = false, message = "Định dạng ảnh không hợp lệ" });
                        }
                        
                        startIndex += dataPrefix.Length;
                        int endIndex = base64Data.IndexOf(";base64,", startIndex);
                        if (endIndex < 0)
                        {
                            _logger.LogWarning("UpdateAvatar: Invalid data format");
                            return Json(new { success = false, message = "Định dạng dữ liệu không hợp lệ" });
                        }
                        
                        string fileExtension = base64Data.Substring(startIndex, endIndex - startIndex);
                        if (!new[] { "jpg", "jpeg", "png", "gif" }.Contains(fileExtension.ToLower()))
                        {
                            _logger.LogWarning($"UpdateAvatar: Unsupported file extension: {fileExtension}");
                            return Json(new { success = false, message = "Chỉ hỗ trợ định dạng JPG, JPEG, PNG, GIF" });
                        }

                        int dataIndex = base64Data.IndexOf("base64,") + "base64,".Length;
                        string imageData = base64Data.Substring(dataIndex);
                        
                        // Ensure avatar directory exists
                        string uploadsFolder = Path.Combine(_webRootPath, "images", "avatar");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                            _logger.LogInformation($"UpdateAvatar: Created avatar directory at {uploadsFolder}");
                        }
                        
                        // Generate unique filename
                        string fileName = $"{user.Id}_{DateTime.Now:yyyyMMddHHmmss}.{fileExtension}";
                        string filePath = Path.Combine(uploadsFolder, fileName);
                        
                        // Save file
                        byte[] imageBytes = Convert.FromBase64String(imageData);
                        await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);
                        
                        _logger.LogInformation($"UpdateAvatar: Saved image to {filePath}");
                        
                        // Update user avatar path
                        string avatarPath = $"/images/avatar/{fileName}";
                        bool result = await _userRepository.UpdateUserAvatarAsync(user.Id, avatarPath);
                        
                        if (result)
                        {
                            _logger.LogInformation($"UpdateAvatar: Updated avatar to {avatarPath} for user ID: {user.Id}");
                            
                            // Set cache control headers
                            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
                            Response.Headers["Pragma"] = "no-cache";
                            Response.Headers["Expires"] = "0";
                            
                            return Json(new { 
                                success = true, 
                                avatarUrl = avatarPath + $"?v={DateTime.Now.Ticks}",
                                message = "Cập nhật ảnh đại diện thành công!",
                                reload = true
                            });
                        }
                        else
                        {
                            _logger.LogWarning($"UpdateAvatar: Failed to update avatar in database for user ID: {user.Id}");
                            return Json(new { success = false, message = "Lỗi cập nhật vào database" });
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"UpdateAvatar: Error processing avatar data: {ex.Message}, StackTrace: {ex.StackTrace}");
                        return Json(new { success = false, message = $"Lỗi khi lưu ảnh: {ex.Message}" });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"UpdateAvatar: Error: {ex.Message}, StackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ChangePassword()
        {
            await Task.CompletedTask;
            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    var user = await _userRepository.GetByUserIdAsync(userId);

                    if (user != null)
                    {
                        bool isCurrentPasswordValid = false;
                        if (user.PasswordHash?.StartsWith("$2") == true)
                        {
                            isCurrentPasswordValid = BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.PasswordHash);
                        }
                        else
                        {
                            isCurrentPasswordValid = (user.PasswordHash == model.CurrentPassword);
                        }

                        if (!isCurrentPasswordValid)
                        {
                            ModelState.AddModelError("CurrentPassword", "Mật khẩu hiện tại không đúng");
                            return View(model);
                        }

                        string newPasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
                        
                        bool updateResult = await _userRepository.UpdatePasswordAsync(userId, newPasswordHash);
                        
                        if (updateResult)
                        {
                            TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
                            return RedirectToAction("Profile");
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Không thể cập nhật mật khẩu.";
                        }
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Không tìm thấy thông tin người dùng.";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"ChangePassword: Error: {ex.Message}, StackTrace: {ex.StackTrace}");
                    TempData["ErrorMessage"] = "Đã xảy ra lỗi khi đổi mật khẩu.";
                }
            }

            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ManageUsers()
        {
            try
            {
                var users = await _userRepository.GetAllUsersAsync();
                return View(users);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ManageUsers: Error: {ex.Message}, StackTrace: {ex.StackTrace}");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi tải danh sách người dùng.";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditUser(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userRepository.GetByUserIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditUser(UserModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var existingUser = await _userRepository.GetByUserIdAsync(model.Id);
                    if (existingUser == null)
                    {
                        return NotFound();
                    }

                    existingUser.FullName = model.FullName;
                    existingUser.Username = model.Username;
                    existingUser.Email = model.Email;
                    existingUser.Phone = model.Phone;
                    existingUser.Address = model.Address;
                    existingUser.Gender = model.Gender;
                    existingUser.DateOfBirth = model.DateOfBirth;
                    existingUser.Bio = model.Bio;
                    existingUser.Level = model.Level;
                    existingUser.Role = model.Role;
                    existingUser.Roles = model.Roles;
                    existingUser.Points = model.Points;
                    
                    bool updateResult = await _userRepository.UpdateUserAsync(existingUser);
                    
                    if (updateResult)
                    {
                        TempData["SuccessMessage"] = "Cập nhật thông tin người dùng thành công!";
                        return RedirectToAction("ManageUsers");
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Không thể cập nhật thông tin người dùng.";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"EditUser: Error: {ex.Message}, StackTrace: {ex.StackTrace}");
                    TempData["ErrorMessage"] = "Đã xảy ra lỗi khi cập nhật thông tin người dùng.";
                }
            }

            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userRepository.GetByUserIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ConfirmDeleteUser(string id)
        {
            try
            {
                bool deleteResult = await _userRepository.DeleteUserAsync(id);
                
                if (deleteResult)
                {
                    TempData["SuccessMessage"] = "Xóa người dùng thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể xóa người dùng.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"ConfirmDeleteUser: Error: {ex.Message}, StackTrace: {ex.StackTrace}");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi xóa người dùng.";
            }

            return RedirectToAction("ManageUsers");
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult CreateUser()
        {
            return View(new UserCreateViewModel());
        }
        
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateUser(UserCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var existingUser = await _userRepository.GetByEmailAsync(model.Email);
                    if (existingUser != null)
                    {
                        ModelState.AddModelError("Email", "Email đã được sử dụng.");
                        return View(model);
                    }

                    var user = new UserModel
                    {
                        Email = model.Email,
                        Username = model.Username,
                        UserName = model.Username,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                        FullName = model.FullName,
                        Avatar = "/images/avatar/default.jpg",
                        Role = model.Role,
                        Roles = new List<string> { model.Role },
                        Level = model.Level ?? "A1",
                        Points = model.Points,
                        CreatedAt = DateTime.Now,
                        RegisterDate = DateTime.Now,
                        IsVerified = true
                    };

                    await _mongoDbService.GetCollection<UserModel>("Users").InsertOneAsync(user);
                    
                    TempData["SuccessMessage"] = "Tạo người dùng mới thành công!";
                    return RedirectToAction("ManageUsers");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"CreateUser: Error: {ex.Message}, StackTrace: {ex.StackTrace}");
                    TempData["ErrorMessage"] = "Đã xảy ra lỗi khi tạo người dùng mới.";
                }
            }
            
            return View(model);
        }

        [Authorize]
        [HttpGet("Account/Debug")]
        public async Task<IActionResult> DebugUserInfo()
        {
            try
            {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                _logger.LogInformation($"DebugUserInfo: User ID: {userId}");
                
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "Không thể xác định người dùng";
                    return RedirectToAction("Profile");
                }

                var user = await _userRepository.GetByUserIdAsync(userId);
                if (user == null)
                {
                    var email = User.FindFirstValue(ClaimTypes.Email);
                    if (!string.IsNullOrEmpty(email))
                    {
                        user = await _userRepository.GetByEmailAsync(email);
                    }
                }

                if (user == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin người dùng";
                    return RedirectToAction("Profile");
                }

                return View("Debug", user);
            }
            catch (Exception ex)
            {
                _logger.LogError($"DebugUserInfo: Error: {ex.Message}, StackTrace: {ex.StackTrace}");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi lấy thông tin người dùng";
                return RedirectToAction("Profile");
            }
        }

        [HttpPost]
        public IActionResult ExternalLogin(string provider)
        {
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["SuccessMessage"] = "Bạn đã đăng xuất thành công.";
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
    
    public class AvatarUpdateModel
    {
        public string? AvatarData { get; set; }
    }
}