using MongoDB.Driver;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;
using TiengAnh.Models;
using TiengAnh.Services;
using System;
using System.IO;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

namespace TiengAnh.Repositories
{
    public class UserRepository : BaseRepository<UserModel>
    {
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(MongoDbService mongoDbService, ILogger<UserRepository> logger) : base(mongoDbService, "Users")
        {
            _logger = logger;
        }

        public async Task<UserModel> GetByEmailAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                Console.WriteLine("GetByEmailAsync: Email is null or empty");
                return null;
            }

            try
            {
                Console.WriteLine($"GetByEmailAsync: Querying for email: {email}");
                var user = await _collection.Find(x => x.Email.ToLower() == email.ToLower()).FirstOrDefaultAsync();
                Console.WriteLine(user != null
                    ? $"GetByEmailAsync: Found user - ID: {user.Id}, UserId: {user.UserId}, Avatar: {user.Avatar}"
                    : $"GetByEmailAsync: No user found for email: {email}");
                return user;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetByEmailAsync: Error: {ex.Message}, StackTrace: {ex.StackTrace}");
                return null;
            }
        }

        public async Task<UserModel> GetByUserIdAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                Console.WriteLine("GetByUserIdAsync: userId is null or empty");
                return null;
            }

            try
            {
                Console.WriteLine($"GetByUserIdAsync: Querying for userId: {userId}");
                var filter = Builders<UserModel>.Filter.Or(
                    Builders<UserModel>.Filter.Eq(u => u.Id, userId),
                    Builders<UserModel>.Filter.Eq(u => u.UserId, userId),
                    Builders<UserModel>.Filter.Eq("_id", ObjectId.Parse(userId))
                );
                var user = await _collection.Find(filter).FirstOrDefaultAsync();
                Console.WriteLine(user != null
                    ? $"GetByUserIdAsync: Found user - ID: {user.Id}, UserId: {user.UserId}, Avatar: {user.Avatar}"
                    : $"GetByUserIdAsync: No user found for userId: {userId}");
                return user;
            }
            catch (FormatException)
            {
                // Nếu userId không phải ObjectId, thử lại mà không parse _id
                Console.WriteLine($"GetByUserIdAsync: userId {userId} is not a valid ObjectId, trying alternative query");
                var filter = Builders<UserModel>.Filter.Or(
                    Builders<UserModel>.Filter.Eq(u => u.Id, userId),
                    Builders<UserModel>.Filter.Eq(u => u.UserId, userId)
                );
                var user = await _collection.Find(filter).FirstOrDefaultAsync();
                Console.WriteLine(user != null
                    ? $"GetByUserIdAsync: Found user - ID: {user.Id}, UserId: {user.UserId}, Avatar: {user.Avatar}"
                    : $"GetByUserIdAsync: No user found for userId: {userId}");
                return user;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetByUserIdAsync: Error: {ex.Message}, StackTrace: {ex.StackTrace}");
                return null;
            }
        }

        public async Task<List<UserModel>> GetAllUsersAsync()
        {
            try
            {
                Console.WriteLine("GetAllUsersAsync: Querying all users");
                var users = await _collection.Find(_ => true).ToListAsync();
                Console.WriteLine($"GetAllUsersAsync: Found {users.Count} users");
                return users;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetAllUsersAsync: Error: {ex.Message}, StackTrace: {ex.StackTrace}");
                return new List<UserModel>();
            }
        }

        public async Task<bool> UpdateUserAsync(UserModel user)
        {
            try
            {
                _logger.LogInformation($"UpdateUserAsync: Updating user - ID: {user.Id}, UserId: {user.UserId}, Email: {user.Email}, Avatar: {user.Avatar}");

                if (user == null || (string.IsNullOrEmpty(user.Id) && string.IsNullOrEmpty(user.UserId) && string.IsNullOrEmpty(user.Email)))
                {
                    _logger.LogWarning("UpdateUserAsync: Invalid user data");
                    return false;
                }

                var filter = Builders<UserModel>.Filter.Empty;

                // Use Id as primary identifier if available
                if (!string.IsNullOrEmpty(user.Id))
                {
                    _logger.LogInformation($"UpdateUserAsync: Using Id: {user.Id} for filter");
                    filter = Builders<UserModel>.Filter.Eq(u => u.Id, user.Id);
                }
                // Otherwise use UserId
                else if (!string.IsNullOrEmpty(user.UserId))
                {
                    _logger.LogInformation($"UpdateUserAsync: Using UserId: {user.UserId} for filter");
                    filter = Builders<UserModel>.Filter.Eq(u => u.UserId, user.UserId);
                }
                // Fall back to Email
                else if (!string.IsNullOrEmpty(user.Email))
                {
                    _logger.LogInformation($"UpdateUserAsync: Using Email: {user.Email} for filter");
                    filter = Builders<UserModel>.Filter.Eq(u => u.Email, user.Email);
                }

                var result = await _collection.ReplaceOneAsync(filter, user, new ReplaceOptions { IsUpsert = false });
                
                _logger.LogInformation($"UpdateUserAsync: Update result - Matched: {result.MatchedCount}, Modified: {result.ModifiedCount}");
                
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateUserAsync");
                return false;
            }
        }

        public async Task<bool> UpdateUserAvatarAsync(string userId, string avatarPath)
        {
            try
            {
                _logger.LogInformation($"UpdateUserAvatarAsync: Setting avatar {avatarPath} for user {userId}");
                
                var filter = Builders<UserModel>.Filter.Empty;
                
                if (!string.IsNullOrEmpty(userId))
                {
                    // Try to match by Id first
                    if (ObjectId.TryParse(userId, out _))
                        filter = Builders<UserModel>.Filter.Eq(u => u.Id, userId);
                    else
                        filter = Builders<UserModel>.Filter.Eq(u => u.UserId, userId);
                }
                else
                {
                    _logger.LogWarning("UpdateUserAvatarAsync: Invalid user ID");
                    return false;
                }
                
                var update = Builders<UserModel>.Update.Set(u => u.Avatar, avatarPath);
                var result = await _collection.UpdateOneAsync(filter, update);
                
                _logger.LogInformation($"UpdateUserAvatarAsync: Update result - Matched: {result.MatchedCount}, Modified: {result.ModifiedCount}");
                
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in UpdateUserAvatarAsync for user {userId}");
                return false;
            }
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                Console.WriteLine("DeleteUserAsync: userId is null or empty");
                return false;
            }

            try
            {
                Console.WriteLine($"DeleteUserAsync: Deleting user with userId: {userId}");
                var filter = Builders<UserModel>.Filter.Empty;

                try
                {
                    var objectId = new ObjectId(userId);
                    filter = Builders<UserModel>.Filter.Eq("_id", objectId);
                }
                catch
                {
                    filter = Builders<UserModel>.Filter.Or(
                        Builders<UserModel>.Filter.Eq(u => u.Id, userId),
                        Builders<UserModel>.Filter.Eq(u => u.UserId, userId)
                    );
                }

                var result = await _collection.DeleteOneAsync(filter);
                Console.WriteLine($"DeleteUserAsync: Result - DeletedCount: {result.DeletedCount}");
                return result.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DeleteUserAsync: Error: {ex.Message}, StackTrace: {ex.StackTrace}");
                return false;
            }
        }

        public async Task<bool> UpdatePasswordAsync(string userId, string hashedPassword)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(hashedPassword))
            {
                Console.WriteLine("UpdatePasswordAsync: userId or hashedPassword is null or empty");
                return false;
            }

            try
            {
                Console.WriteLine($"UpdatePasswordAsync: Updating password for userId: {userId}");
                var filter = Builders<UserModel>.Filter.Empty;

                try
                {
                    var objectId = new ObjectId(userId);
                    filter = Builders<UserModel>.Filter.Eq("_id", objectId);
                }
                catch
                {
                    filter = Builders<UserModel>.Filter.Or(
                        Builders<UserModel>.Filter.Eq(u => u.Id, userId),
                        Builders<UserModel>.Filter.Eq(u => u.UserId, userId)
                    );
                }

                var update = Builders<UserModel>.Update
                    .Set(u => u.PasswordHash, hashedPassword);

                var result = await _collection.UpdateOneAsync(filter, update);
                Console.WriteLine($"UpdatePasswordAsync: Result - MatchedCount: {result.MatchedCount}, ModifiedCount: {result.ModifiedCount}");
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UpdatePasswordAsync: Error: {ex.Message}, StackTrace: {ex.StackTrace}");
                return false;
            }
        }

        public async Task<bool> ExportUsersToJsonAsync(string filePath)
        {
            try
            {
                Console.WriteLine($"ExportUsersToJsonAsync: Exporting users to {filePath}");
                var users = await GetAllUsersAsync();
                string json = JsonConvert.SerializeObject(users, Formatting.Indented);
                await File.WriteAllTextAsync(filePath, json);
                Console.WriteLine("ExportUsersToJsonAsync: Export successful");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ExportUsersToJsonAsync: Error: {ex.Message}, StackTrace: {ex.StackTrace}");
                return false;
            }
        }
    }
}