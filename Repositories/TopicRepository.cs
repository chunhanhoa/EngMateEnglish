using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using TiengAnh.Models;
using TiengAnh.Services;

namespace TiengAnh.Repositories
{
    public class TopicRepository : ITopicRepository
    {
        private readonly MongoDbService _mongoDbService;
        private readonly IMongoCollection<TopicModel> _topicsCollection;

        public TopicRepository(MongoDbService mongoDbService)
        {
            _mongoDbService = mongoDbService;
            _topicsCollection = _mongoDbService.GetCollection<TopicModel>("Topics");
        }

        public async Task<List<TopicModel>> GetAllAsync()
        {
            try
            {
                return await _topicsCollection.Find(_ => true).ToListAsync();
            }
            catch
            {
                // Trả về danh sách rỗng nếu có lỗi
                return new List<TopicModel>();
            }
        }

        public async Task<TopicModel?> GetByIdAsync(string id)
        {
            // Thử chuyển đổi id thành số nếu được
            if (int.TryParse(id, out int idAsInt))
            {
                return await _topicsCollection.Find(t => t.ID_CD == idAsInt).FirstOrDefaultAsync();
            }
            else
            {
                // Nếu không phải số, có thể đây là ID_CD dạng chuỗi
                return await _topicsCollection.Find(t => t.ID_CD.ToString() == id).FirstOrDefaultAsync();
            }
        }

        public async Task<bool> CreateAsync(TopicModel topic)
        {
            try
            {
                await _topicsCollection.InsertOneAsync(topic);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateAsync(string id, TopicModel topic)
        {
            try
            {
                // Thử chuyển đổi id thành số nếu được
                if (int.TryParse(id, out int idAsInt))
                {
                    var result = await _topicsCollection.ReplaceOneAsync(t => t.ID_CD == idAsInt, topic);
                    return result.IsAcknowledged && result.ModifiedCount > 0;
                }
                else
                {
                    var result = await _topicsCollection.ReplaceOneAsync(t => t.ID_CD.ToString() == id, topic);
                    return result.IsAcknowledged && result.ModifiedCount > 0;
                }
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                // Thử chuyển đổi id thành số nếu được
                if (int.TryParse(id, out int idAsInt))
                {
                    var result = await _topicsCollection.DeleteOneAsync(t => t.ID_CD == idAsInt);
                    return result.IsAcknowledged && result.DeletedCount > 0;
                }
                else
                {
                    var result = await _topicsCollection.DeleteOneAsync(t => t.ID_CD.ToString() == id);
                    return result.IsAcknowledged && result.DeletedCount > 0;
                }
            }
            catch
            {
                return false;
            }
        }
        
        // Thêm các phương thức bị thiếu
        public async Task<List<TopicModel>> GetAllTopicsAsync()
        {
            return await GetAllAsync();
        }
        
        public async Task<TopicModel?> GetTopicByIdAsync(string id)
        {
            return await GetByIdAsync(id);
        }
        
        public async Task<TopicModel?> GetByTopicIdAsync(string id)
        {
            return await GetByIdAsync(id);
        }
        
        public async Task<TopicModel?> GetByTopicIdAsync(int id)
        {
            return await _topicsCollection.Find(t => t.ID_CD == id).FirstOrDefaultAsync();
        }
        
        public async Task<TopicModel?> GetTopicByIdAsync(int id)
        {
            return await _topicsCollection.Find(t => t.ID_CD == id).FirstOrDefaultAsync();
        }
        
        public async Task<bool> HasDataAsync()
        {
            var count = await _topicsCollection.CountDocumentsAsync(_ => true);
            return count > 0;
        }
    }
}
