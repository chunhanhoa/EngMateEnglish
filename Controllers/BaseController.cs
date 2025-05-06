using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MongoDB.Driver;
using MongoDB.Bson;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using TiengAnh.Models;
using TiengAnh.Services;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Logging;
using System.Linq;
using TiengAnh.Repositories;

namespace TiengAnh.Controllers
{
    public abstract class BaseController : Controller
    {
        protected readonly MongoDbService _mongoDbService;
        protected readonly UserRepository _userRepository;
        private readonly ILogger<BaseController> _logger;

        public BaseController(MongoDbService mongoDbService)
        {
            _mongoDbService = mongoDbService;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Thêm header để tránh cache
            Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, max-age=0";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";

            if (User.Identity?.IsAuthenticated == true)
            {
                try
                {
                    // Lấy thông tin người dùng từ claim
                    string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    string? email = User.FindFirstValue(ClaimTypes.Email);
                    string? username = User.FindFirstValue(ClaimTypes.Name);

                    // LƯU Ý: LUÔN LUÔN TÌM TRỰC TIẾP TỪ DATABASE, ĐỪNG DÙNG CACHE!
                    UserModel user = null;
                    
                    if (!string.IsNullOrEmpty(userId) && _userRepository != null)
                    {
                        user = await _userRepository.GetByUserIdAsync(userId);
                    }
                    
                    if (user == null && !string.IsNullOrEmpty(email) && _userRepository != null)
                    {
                        user = await _userRepository.GetByEmailAsync(email);
                    }
                    
                    if (user == null && !string.IsNullOrEmpty(username) && _mongoDbService != null)
                    {
                        // Tìm theo username nếu cần
                        var filter = Builders<UserModel>.Filter.Eq(u => u.Username, username);
                        user = await _mongoDbService.GetCollection<UserModel>("Users")
                            .Find(filter)
                            .FirstOrDefaultAsync();
                    }

                    string defaultAvatarPath = "/images/default-avatar.png";
                    
                    if (user != null)
                    {
                        // Thêm timestamp để tránh browser cache
                        var timestamp = DateTime.Now.Ticks;
                        
                        // Thiết lập đường dẫn avatar
                        string avatarPath = !string.IsNullOrEmpty(user.Avatar) ? user.Avatar : defaultAvatarPath;
                        
                        if (!string.IsNullOrEmpty(avatarPath) && !avatarPath.StartsWith("/"))
                        {
                            avatarPath = "/" + avatarPath;
                        }

                        // Thiết lập ViewBag với dữ liệu mới
                        ViewBag.CurrentUser = user;
                        ViewBag.CurrentUserId = user.Id;
                        ViewBag.CurrentUserName = user.Username;
                        ViewBag.CurrentUserFullName = user.FullName;
                        ViewBag.UserAvatar = avatarPath;
                        ViewBag.UserAvatarWithTimestamp = $"{avatarPath}?v={timestamp}";
                        ViewBag.UserEmail = user.Email;
                    }
                    else
                    {
                        // Không tìm thấy user
                        ViewBag.UserAvatar = defaultAvatarPath;
                        ViewBag.UserAvatarWithTimestamp = $"{defaultAvatarPath}?v={DateTime.Now.Ticks}";
                    }
                }
                catch (Exception ex)
                {
                    // Lỗi xử lý
                    ViewBag.UserAvatar = "/images/default-avatar.png";
                    ViewBag.UserAvatarWithTimestamp = $"/images/default-avatar.png?v={DateTime.Now.Ticks}";
                }
            }
            
            await next();
        }
    }
}
