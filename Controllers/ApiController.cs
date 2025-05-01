using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TiengAnh.Models;
using TiengAnh.Repositories;
using TiengAnh.Services;

namespace TiengAnh.Controllers
{
    [Route("api")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class ApiController : ControllerBase
    {
        private readonly MongoDbService _mongoDbService;
        private readonly ILogger<ApiController> _logger;
        private readonly UserRepository _userRepository;

        public ApiController(MongoDbService mongoDbService, ILogger<ApiController> logger, UserRepository userRepository)
        {
            _mongoDbService = mongoDbService;
            _logger = logger;
            _userRepository = userRepository;
        }

        // GET: api/test-connection
        [HttpGet("test-connection")]
        public IActionResult TestConnection()
        {
            try
            {
                return Ok(new { Status = "OK", Message = "MongoDB connection is working" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"MongoDB connection error: {ex.Message}");
                return StatusCode(500, new { Status = "Error", Message = ex.Message });
            }
        }

        // GET: api/users/nhanhoa@gmail.com
        [HttpGet("users/{email}")]
        public async Task<IActionResult> GetUserByEmail(string email)
        {
            try
            {
                var usersCollection = _mongoDbService.GetCollection<UserModel>("Users");
                var user = await usersCollection.Find(u => u.Email == email).FirstOrDefaultAsync();

                if (user == null)
                {
                    return NotFound(new { Status = "NotFound", Message = $"User with email {email} not found" });
                }

                // Chỉ trả về các thông tin cần thiết
                return Ok(new { 
                    Status = "OK",
                    UserId = user.UserId,
                    Id = user.Id,
                    Email = user.Email,
                    Username = user.Username,
                    Avatar = user.Avatar,
                    FullName = user.FullName
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting user: {ex.Message}");
                return StatusCode(500, new { Status = "Error", Message = ex.Message });
            }
        }

        // POST: api/update-avatar
        [HttpPost("update-avatar")]
        public async Task<IActionResult> UpdateUserAvatar([FromBody] UpdateAvatarRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.AvatarPath))
                {
                    return BadRequest(new { Status = "Error", Message = "Email and AvatarPath are required" });
                }

                var usersCollection = _mongoDbService.GetCollection<UserModel>("Users");
                var filter = Builders<UserModel>.Filter.Eq(u => u.Email, request.Email);
                var update = Builders<UserModel>.Update.Set(u => u.Avatar, request.AvatarPath);

                var result = await usersCollection.UpdateOneAsync(filter, update);

                if (result.MatchedCount == 0)
                {
                    return NotFound(new { Status = "NotFound", Message = $"User with email {request.Email} not found" });
                }

                return Ok(new { 
                    Status = "OK", 
                    MatchedCount = result.MatchedCount,
                    ModifiedCount = result.ModifiedCount,
                    Message = "Avatar updated successfully" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating avatar: {ex.Message}");
                return StatusCode(500, new { Status = "Error", Message = ex.Message });
            }
        }

        // GET: api/export-users
        [HttpGet("export-users")]
        public async Task<IActionResult> ExportUsers()
        {
            try
            {
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "json", "users.json");
                
                bool result = await _userRepository.ExportUsersToJsonAsync(filePath);
                
                if (result)
                {
                    return Ok(new { success = true, message = "Đã xuất file users.json thành công", path = filePath });
                }
                else
                {
                    return BadRequest(new { success = false, message = "Không thể xuất file users.json" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error exporting users: {ex.Message}");
                return StatusCode(500, new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }
    }

    public class UpdateAvatarRequest
    {
        public required string Email { get; set; }
        public required string AvatarPath { get; set; }
    }
}
