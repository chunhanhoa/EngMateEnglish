using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using TiengAnh.Models;
using TiengAnh.Services;

namespace TiengAnh.Repositories
{
    public class TopicRepository : BaseRepository<TopicModel>, ITopicRepository
    {
        public TopicRepository(MongoDbService mongoDbService) : base(mongoDbService, "Topics")
        {
        }

        public async Task<List<TopicModel>> GetAllAsync()
        {
            try
            {
                return await _collection.Find(_ => true).ToListAsync();
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
                return await _collection.Find(t => t.ID_CD == idAsInt).FirstOrDefaultAsync();
            }
            else
            {
                // Nếu không phải số, có thể đây là ID_CD dạng chuỗi
                return await _collection.Find(t => t.ID_CD.ToString() == id).FirstOrDefaultAsync();
            }
        }

        public async Task<bool> CreateAsync(TopicModel topic)
        {
            try
            {
                await _collection.InsertOneAsync(topic);
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
                    var result = await _collection.ReplaceOneAsync(t => t.ID_CD == idAsInt, topic);
                    return result.IsAcknowledged && result.ModifiedCount > 0;
                }
                else
                {
                    var result = await _collection.ReplaceOneAsync(t => t.ID_CD.ToString() == id, topic);
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
                    var result = await _collection.DeleteOneAsync(t => t.ID_CD == idAsInt);
                    return result.IsAcknowledged && result.DeletedCount > 0;
                }
                else
                {
                    var result = await _collection.DeleteOneAsync(t => t.ID_CD.ToString() == id);
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
            return await _collection.Find(t => t.ID_CD == id).FirstOrDefaultAsync();
        }

        public async Task<TopicModel?> GetTopicByIdAsync(int id)
        {
            return await _collection.Find(t => t.ID_CD == id).FirstOrDefaultAsync();
        }

        public async Task<bool> HasDataAsync()
        {
            var count = await _collection.CountDocumentsAsync(_ => true);
            return count > 0;
        }

        // Kiểm tra xem chủ đề có được yêu thích bởi người dùng hay không
        public async Task<bool> IsTopicFavoriteByUserAsync(int topicId, string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return false;

            var topic = await _collection.Find(x => x.ID_CD == topicId).FirstOrDefaultAsync();
            if (topic == null || topic.FavoriteByUsers == null)
                return false;

            return topic.FavoriteByUsers.Contains(userId);
        }

        // Chức năng toggle yêu thích chủ đề
        public async Task<bool> ToggleFavoriteAsync(int topicId, string userId)
        {
            try
            {
                var topic = await _collection.Find(x => x.ID_CD == topicId).FirstOrDefaultAsync();
                if (topic == null)
                {
                    return false;
                }

                // Khởi tạo danh sách người dùng yêu thích nếu chưa có
                if (topic.FavoriteByUsers == null)
                {
                    topic.FavoriteByUsers = new List<string>();
                }

                // Kiểm tra xem người dùng đã thêm vào yêu thích chưa
                bool isFavorited = topic.FavoriteByUsers.Contains(userId);

                UpdateDefinition<TopicModel> update;

                if (isFavorited)
                {
                    // Nếu đã là yêu thích, xóa khỏi danh sách
                    update = Builders<TopicModel>.Update.Pull(x => x.FavoriteByUsers, userId);
                }
                else
                {
                    // Nếu chưa yêu thích, thêm vào danh sách
                    update = Builders<TopicModel>.Update.Push(x => x.FavoriteByUsers, userId);
                }

                var result = await _collection.UpdateOneAsync(x => x.ID_CD == topicId, update);
                return result.ModifiedCount > 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
