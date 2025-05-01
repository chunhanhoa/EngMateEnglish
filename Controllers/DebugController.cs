using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TiengAnh.Models;
using TiengAnh.Repositories;
using TiengAnh.Services;

namespace TiengAnh.Controllers
{
    [Authorize]
    [Route("debug")]
    public class DebugController : Controller
    {
        private readonly UserRepository _userRepository;
        private readonly MongoDbService _mongoDbService;
        private readonly ILogger<DebugController> _logger;
        private readonly IWebHostEnvironment _hostEnvironment;

        public DebugController(UserRepository userRepository, MongoDbService mongoDbService, ILogger<DebugController> logger = null, IWebHostEnvironment hostEnvironment = null)
        {
            _userRepository = userRepository;
            _mongoDbService = mongoDbService;
            _logger = logger;
            _hostEnvironment = hostEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> UserInfo()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var email = User.FindFirstValue(ClaimTypes.Email);

                var userFromId = !string.IsNullOrEmpty(userId) ? 
                    await _userRepository.GetByUserIdAsync(userId) : null;
                var userFromEmail = !string.IsNullOrEmpty(email) ? 
                    await _userRepository.GetByEmailAsync(email) : null;

                return Json(new
                {
                    UserIdFromClaims = userId,
                    EmailFromClaims = email,
                    UserFromId = userFromId != null ? new
                    {
                        Id = userFromId.Id,
                        UserId = userFromId.UserId,
                        Email = userFromId.Email,
                        Avatar = userFromId.Avatar,
                        FullName = userFromId.FullName
                    } : null,
                    UserFromEmail = userFromEmail != null ? new
                    {
                        Id = userFromEmail.Id,
                        UserId = userFromEmail.UserId,
                        Email = userFromEmail.Email,
                        Avatar = userFromEmail.Avatar,
                        FullName = userFromEmail.FullName
                    } : null
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpGet]
        public async Task<IActionResult> TestAvatarUpdate(string userId, string avatarPath)
        {
            try
            {
                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(avatarPath))
                {
                    return BadRequest("UserId and AvatarPath are required");
                }

                var user = await _userRepository.GetByUserIdAsync(userId);
                if (user == null)
                {
                    return NotFound($"User with ID {userId} not found");
                }

                user.Avatar = avatarPath;
                var updateResult = await _userRepository.UpdateUserAsync(user);

                return Json(new
                {
                    Success = updateResult,
                    User = new
                    {
                        Id = user.Id,
                        UserId = user.UserId,
                        Email = user.Email,
                        Avatar = user.Avatar
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpGet("avatar")]
        public async Task<IActionResult> CheckAvatar()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var email = User.FindFirstValue(ClaimTypes.Email);
                
                // Tìm user
                UserModel user = null;
                if (!string.IsNullOrEmpty(userId))
                {
                    user = await _userRepository.GetByUserIdAsync(userId);
                }
                
                if (user == null && !string.IsNullOrEmpty(email))
                {
                    user = await _userRepository.GetByEmailAsync(email);
                }
                
                if (user == null)
                {
                    return Json(new { error = "User not found" });
                }
                
                string avatarPath = user.Avatar;
                bool fileExists = false;
                string physicalPath = "";
                
                if (!string.IsNullOrEmpty(avatarPath))
                {
                    physicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", avatarPath.TrimStart('/'));
                    fileExists = System.IO.File.Exists(physicalPath);
                }
                
                // Kiểm tra tất cả các avatar trong thư mục
                var avatarFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "avatar");
                var allAvatarFiles = Directory.Exists(avatarFolder) ? Directory.GetFiles(avatarFolder) : new string[0];
                
                return Json(new {
                    user = new {
                        id = user.Id,
                        email = user.Email,
                        username = user.Username,
                        avatarPath = avatarPath
                    },
                    avatarExists = fileExists,
                    physicalPath = physicalPath,
                    allAvatars = allAvatarFiles
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }
        
        [HttpGet("fix-avatar")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> FixAvatar(string userId, string avatarPath)
        {
            try 
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest("UserId is required");
                }
                
                var user = await _userRepository.GetByUserIdAsync(userId);
                if (user == null)
                {
                    return NotFound("User not found");
                }
                
                user.Avatar = avatarPath;
                var result = await _userRepository.UpdateUserAsync(user);
                
                return Json(new { 
                    success = result, 
                    user = new {
                        id = user.Id,
                        email = user.Email,
                        username = user.Username,
                        avatarPath = user.Avatar
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet("show-avatar")]
        public IActionResult ShowAvatar()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            ViewBag.UserEmail = userEmail;
            
            return View();
        }

        [HttpGet("check-avatar-view")]
        public async Task<IActionResult> CheckAvatarView()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var email = User.FindFirstValue(ClaimTypes.Email);
                
                // Thông tin user từ claims
                ViewBag.UserIdFromClaim = userId;
                ViewBag.EmailFromClaim = email;
                
                // Kiểm tra thư mục avatar
                string avatarFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "avatar");
                bool folderExists = Directory.Exists(avatarFolder);
                ViewBag.AvatarFolderExists = folderExists;
                ViewBag.AvatarFolderPath = avatarFolder;
                
                if (folderExists)
                {
                    ViewBag.AvatarFiles = Directory.GetFiles(avatarFolder);
                }
                
                // Tìm user
                var user = await _userRepository.GetByUserIdAsync(userId);
                if (user == null && !string.IsNullOrEmpty(email))
                {
                    user = await _userRepository.GetByEmailAsync(email);
                }
                
                if (user != null)
                {
                    ViewBag.UserFound = true;
                    ViewBag.User = user;
                    
                    // Kiểm tra file avatar
                    if (!string.IsNullOrEmpty(user.Avatar))
                    {
                        string avatarPath = user.Avatar;
                        if (!avatarPath.StartsWith("/"))
                            avatarPath = "/" + avatarPath;
                            
                        string physicalPath = Path.Combine(
                            Directory.GetCurrentDirectory(), 
                            "wwwroot", 
                            avatarPath.TrimStart('/')
                        );
                        
                        bool fileExists = System.IO.File.Exists(physicalPath);
                        ViewBag.AvatarFileExists = fileExists;
                        ViewBag.AvatarPath = avatarPath;
                        ViewBag.AvatarPhysicalPath = physicalPath;
                    }
                    else
                    {
                        ViewBag.AvatarMessage = "User doesn't have avatar set";
                    }
                }
                else
                {
                    ViewBag.UserFound = false;
                    ViewBag.ErrorMessage = "User not found";
                }
                
                return View("CheckAvatar");
            }
            catch (Exception ex)
            {
                return Content($"Error: {ex.Message}\nStack: {ex.StackTrace}");
            }
        }

        [AllowAnonymous]
        [HttpGet("get-avatar-path")]
        public async Task<IActionResult> GetAvatarPath()
        {
            try
            {
                if (!User.Identity?.IsAuthenticated == true)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                string? email = User.FindFirstValue(ClaimTypes.Email);
                string? username = User.Identity.Name;

                // Debug information
                Console.WriteLine($"GetAvatarPath: userId={userId}, email={email}, username={username}");

                // Tìm user với nhiều cách khác nhau để đảm bảo tìm được
                UserModel user = null;
                
                // Cách 1: Tìm theo userId
                if (!string.IsNullOrEmpty(userId) && _userRepository != null)
                {
                    user = await _userRepository.GetByUserIdAsync(userId);
                    Console.WriteLine($"Tìm user theo userId: {userId}, Kết quả: {user?.Id}");
                }
                
                // Cách 2: Tìm theo email
                if (user == null && !string.IsNullOrEmpty(email) && _userRepository != null)
                {
                    user = await _userRepository.GetByEmailAsync(email);
                    Console.WriteLine($"Tìm user theo email: {email}, Kết quả: {user?.Id}");
                }
                
                // Cách 3: Tìm theo username
                if (user == null && !string.IsNullOrEmpty(username) && _mongoDbService != null)
                {
                    // Thử tìm chính xác
                    var filter = Builders<UserModel>.Filter.Eq(u => u.Username, username);
                    user = await _mongoDbService.GetCollection<UserModel>("Users")
                        .Find(filter)
                        .FirstOrDefaultAsync();
                        
                    // Nếu không tìm thấy, thử tìm theo UserName
                    if (user == null)
                    {
                        filter = Builders<UserModel>.Filter.Eq(u => u.UserName, username);
                        user = await _mongoDbService.GetCollection<UserModel>("Users")
                            .Find(filter)
                            .FirstOrDefaultAsync();
                    }
                    
                    Console.WriteLine($"Tìm user theo username: {username}, Kết quả: {user?.Id}");
                }
                
                // Cách 4: Tìm tất cả người dùng và lọc
                if (user == null && _mongoDbService != null)
                {
                    var allUsers = await _mongoDbService.GetCollection<UserModel>("Users")
                        .Find(Builders<UserModel>.Filter.Empty)
                        .ToListAsync();
                        
                    Console.WriteLine($"Tìm thấy {allUsers.Count} người dùng tổng cộng");
                    
                    user = allUsers.FirstOrDefault(u => 
                        (userId != null && u.UserId == userId) || 
                        (email != null && u.Email == email) ||
                        (username != null && (u.Username == username || u.UserName == username)));
                        
                    if (user != null)
                    {
                        Console.WriteLine($"Tìm thấy user thông qua lọc danh sách: {user.Id}, {user.Username}");
                    }
                }

                if (user == null)
                {
                    Console.WriteLine("KHÔNG TÌM THẤY NGƯỜI DÙNG");
                    return Json(new { 
                        success = false, 
                        message = "User not found",
                        userId = userId,
                        email = email,
                        username = username
                    });
                }

                // Kiểm tra avatar path
                string avatarPath = !string.IsNullOrEmpty(user.Avatar) ? user.Avatar : "/images/default-avatar.png";
                
                // Đảm bảo có dấu / ở đầu
                if (!string.IsNullOrEmpty(avatarPath) && !avatarPath.StartsWith("/"))
                {
                    avatarPath = "/" + avatarPath;
                }

                // Kiểm tra tệp có tồn tại không
                string fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", avatarPath.TrimStart('/'));
                bool fileExists = System.IO.File.Exists(fullPath);
                
                // Nếu tệp không tồn tại, sử dụng avatar mặc định
                if (!fileExists && !avatarPath.Contains("default-avatar"))
                {
                    Console.WriteLine($"File avatar không tồn tại, chuyển về mặc định: {avatarPath}");
                    avatarPath = "/images/default-avatar.png";
                }

                // Tắt cache
                Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
                Response.Headers["Pragma"] = "no-cache";
                Response.Headers["Expires"] = "0";

                Console.WriteLine($"Trả về avatar: {avatarPath} cho user {user.Username}");
                
                return Json(new { 
                    success = true, 
                    avatarPath = avatarPath,
                    userId = user.Id,
                    username = user.Username,
                    email = user.Email,
                    timestamp = DateTime.Now.Ticks,
                    fileExists = fileExists
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi trong GetAvatarPath: {ex.Message}, {ex.StackTrace}");
                _logger?.LogError(ex, "Error in GetAvatarPath");
                return Json(new { success = false, error = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpGet("debug/check-avatar-path")]
        public async Task<IActionResult> CheckUserAvatarPath()
        {
            // Lấy email từ người dùng hiện tại
            var userEmail = User.Identity.IsAuthenticated ? User.Identity.Name : null;
            
            if (string.IsNullOrEmpty(userEmail))
            {
                return Json(new { success = false, message = "Bạn chưa đăng nhập" });
            }
            
            // Lấy thông tin người dùng từ database
            var usersCollection = _mongoDbService.GetCollection<UserModel>("Users");
            var user = await usersCollection.Find(u => u.Email == userEmail).FirstOrDefaultAsync();
            
            if (user == null)
            {
                return Json(new { success = false, message = "Không tìm thấy người dùng" });
            }
            
            // Kiểm tra avatar path
            bool avatarFileExists = false;
            string fullPath = "";
            
            if (!string.IsNullOrEmpty(user.Avatar))
            {
                string webRootPath = _hostEnvironment.WebRootPath;
                string relativePath = user.Avatar.TrimStart('/');
                fullPath = Path.Combine(webRootPath, relativePath);
                avatarFileExists = System.IO.File.Exists(fullPath);
            }
            
            return Json(new {
                success = true,
                userEmail = userEmail,
                avatarPath = user.Avatar,
                avatarExists = avatarFileExists,
                fullPath = fullPath
            });
        }

        [AllowAnonymous]
        [HttpGet("debug/get-avatar-url")]
        public async Task<IActionResult> GetCurrentUserAvatar()
        {
            // Lấy email từ người dùng hiện tại
            var userEmail = User.Identity.IsAuthenticated ? User.Identity.Name : null;
            
            if (string.IsNullOrEmpty(userEmail))
            {
                return Json(new { success = false, message = "Bạn chưa đăng nhập" });
            }
            
            // Lấy thông tin người dùng từ database
            var usersCollection = _mongoDbService.GetCollection<UserModel>("Users");
            var user = await usersCollection.Find(u => u.Email == userEmail).FirstOrDefaultAsync();
            
            if (user == null)
            {
                return Json(new { success = false, message = "Không tìm thấy người dùng" });
            }
            
            // Trả về đường dẫn avatar với timestamp
            string avatarPath = !string.IsNullOrEmpty(user.Avatar) ? user.Avatar : "/images/default-avatar.png";
            string timestamp = DateTime.Now.Ticks.ToString();
            
            return Json(new {
                success = true,
                avatarPath = avatarPath,
                avatarWithTimestamp = $"{avatarPath}?v={timestamp}"
            });
        }
    }
}
