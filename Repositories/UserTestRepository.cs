using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TiengAnh.Models;
using TiengAnh.Services;

namespace TiengAnh.Repositories
{
    public class UserTestRepository
    {
        private readonly IMongoCollection<UserTestModel> _collection;
        
        public UserTestRepository(MongoDbService mongoDbService)
        {
            _collection = mongoDbService.GetCollection<UserTestModel>("UserTests");
        }
        
        public async Task<List<UserTestModel>> GetCompletedTestsByUserIdAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return new List<UserTestModel>();
            }
            
            return await _collection.Find(t => t.UserId == userId)
                .SortByDescending(t => t.CompletedAt)
                .ToListAsync();
        }
        
        public async Task<UserTestModel> GetByUserAndTestIdAsync(string userId, string testId)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(testId))
            {
                return null;
            }
            
            return await _collection.Find(t => t.UserId == userId && t.TestId == testId)
                .FirstOrDefaultAsync();
        }
        
        public async Task<bool> SaveUserTestAsync(UserTestModel userTest)
        {
            try
            {
                if (string.IsNullOrEmpty(userTest.Id))
                {
                    await _collection.InsertOneAsync(userTest);
                    return true;
                }
                else
                {
                    var filter = Builders<UserTestModel>.Filter.Eq(t => t.Id, userTest.Id);
                    var result = await _collection.ReplaceOneAsync(filter, userTest);
                    return result.IsAcknowledged && result.ModifiedCount > 0;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
