using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using TiengAnh.Models;
using TiengAnh.Services;

namespace TiengAnh.Repositories
{
    public class VocabularyRepository : BaseRepository<VocabularyModel>
    {
        // Sửa tên collection từ "Vocabulary" thành "Vocabularies"
        public VocabularyRepository(MongoDbService mongoDbService) : base(mongoDbService, "Vocabularies")
        {
        }

        public async Task<VocabularyModel> GetByVocabularyIdAsync(int vocabId)
        {
            return await _collection.Find(x => x.ID_TV == vocabId).FirstOrDefaultAsync();
        }

        public async Task<List<VocabularyModel>> GetByTopicIdAsync(int topicId)
        {
            // Thêm logging
            var results = await _collection.Find(x => x.ID_CD == topicId).ToListAsync();
            // Đảm bảo lấy được dữ liệu
            return results;
        }

        // Thêm alias cho phương thức GetByTopicIdAsync để duy trì tương thích với mã hiện tại
        public async Task<List<VocabularyModel>> GetVocabulariesByTopicIdAsync(int topicId)
        {
            return await GetByTopicIdAsync(topicId);
        }

        // Cập nhật phương thức để lấy danh sách từ vựng yêu thích theo userId
        public async Task<List<VocabularyModel>> GetFavoriteVocabulariesAsync(string userId)
        {
            // Thay vì chỉ lọc theo IsFavorite, bây giờ cần lọc theo FavoriteByUsers chứa userId
            var filter = Builders<VocabularyModel>.Filter.AnyEq(x => x.FavoriteByUsers, userId);
            return await _collection.Find(filter).ToListAsync();
        }

        // Thêm phương thức để thêm/xóa từ vựng khỏi danh sách yêu thích
        public async Task<bool> ToggleFavoriteAsync(int vocabularyId, string userId)
        {
            try
            {
                var vocabulary = await _collection.Find(x => x.ID_TV == vocabularyId).FirstOrDefaultAsync();
                if (vocabulary == null)
                {
                    return false;
                }

                // Khởi tạo danh sách người dùng yêu thích nếu chưa có
                if (vocabulary.FavoriteByUsers == null)
                {
                    vocabulary.FavoriteByUsers = new List<string>();
                }

                // Kiểm tra xem người dùng đã thêm vào yêu thích chưa
                bool isFavorited = vocabulary.FavoriteByUsers.Contains(userId);

                UpdateDefinition<VocabularyModel> update;

                if (isFavorited)
                {
                    // Nếu đã là yêu thích, xóa khỏi danh sách
                    update = Builders<VocabularyModel>.Update.Pull(x => x.FavoriteByUsers, userId);
                }
                else
                {
                    // Nếu chưa yêu thích, thêm vào danh sách
                    update = Builders<VocabularyModel>.Update.Push(x => x.FavoriteByUsers, userId);
                }

                var result = await _collection.UpdateOneAsync(x => x.ID_TV == vocabularyId, update);
                return result.ModifiedCount > 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
