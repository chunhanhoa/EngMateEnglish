using MongoDB.Driver;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using TiengAnh.Models;
using TiengAnh.Services;

namespace TiengAnh.Repositories
{
    public class GrammarRepository : BaseRepository<GrammarModel>
    {
        public GrammarRepository(MongoDbService mongoDbService) : base(mongoDbService, "Grammar")
        {
        }

        public async Task<GrammarModel> GetByGrammarIdAsync(int grammarId)
        {
            // Make sure we're filtering by ID_NP, not the MongoDB ObjectId
            var filter = Builders<GrammarModel>.Filter.Eq(x => x.ID_NP, grammarId);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }
        
        public async Task<Dictionary<string, List<GrammarModel>>> GetGroupedGrammarAsync()
        {
            var grammarList = await _collection.Find(_ => true).ToListAsync();
            return grammarList.GroupBy(g => g.TopicName)
                .ToDictionary(g => g.Key, g => g.OrderBy(x => x.ID_NP).ToList());
        }
        
        public async Task<List<GrammarModel>> GetFavoriteGrammarsAsync(string userId)
        {
            var filter = Builders<GrammarModel>.Filter.AnyEq(x => x.FavoriteByUsers, userId);
            return await _collection.Find(filter).ToListAsync();
        }
        
        public async Task<List<GrammarModel>> GetGrammarsByLevelAsync(string level)
        {
            return await _collection.Find(x => x.Level == level).ToListAsync();
        }
        
        public async Task<Dictionary<string, List<GrammarModel>>> GetGrammarsByLevelGroupAsync()
        {
            var grammarList = await _collection.Find(_ => true).ToListAsync();
            var levelGroups = new Dictionary<string, List<GrammarModel>>();
            
            // Nhóm các bài học ngữ pháp theo trình độ
            var basicLevels = new List<string> { "A1", "A2" };
            var intermediateLevels = new List<string> { "B1", "B2" };
            var advancedLevels = new List<string> { "C1", "C2" };
            
            levelGroups["Basic Grammar"] = grammarList.Where(g => basicLevels.Contains(g.Level)).OrderBy(g => g.ID_NP).ToList();
            levelGroups["Intermediate Grammar"] = grammarList.Where(g => intermediateLevels.Contains(g.Level)).OrderBy(g => g.ID_NP).ToList();
            levelGroups["Advanced Grammar"] = grammarList.Where(g => advancedLevels.Contains(g.Level)).OrderBy(g => g.ID_NP).ToList();
            
            return levelGroups;
        }

        public async Task<bool> ToggleFavoriteAsync(int grammarId, string userId)
        {
            try
            {
                var grammar = await _collection.Find(x => x.ID_NP == grammarId).FirstOrDefaultAsync();
                if (grammar == null)
                {
                    return false;
                }

                // Khởi tạo danh sách người dùng yêu thích nếu chưa có
                if (grammar.FavoriteByUsers == null)
                {
                    grammar.FavoriteByUsers = new List<string>();
                }

                // Kiểm tra xem người dùng đã thêm vào yêu thích chưa
                bool isFavorited = grammar.FavoriteByUsers.Contains(userId);

                UpdateDefinition<GrammarModel> update;

                if (isFavorited)
                {
                    // Nếu đã là yêu thích, xóa khỏi danh sách
                    update = Builders<GrammarModel>.Update.Pull(x => x.FavoriteByUsers, userId);
                }
                else
                {
                    // Nếu chưa yêu thích, thêm vào danh sách
                    update = Builders<GrammarModel>.Update.Push(x => x.FavoriteByUsers, userId);
                }

                var result = await _collection.UpdateOneAsync(x => x.ID_NP == grammarId, update);
                return result.ModifiedCount > 0;
            }
            catch
            {
                return false;
            }
        }

        // Override UpdateAsync to better handle our specific case
        public new async Task UpdateAsync(string id, GrammarModel updatedEntity)
        {
            // Use the MongoDB Object ID for the update
            var filter = Builders<GrammarModel>.Filter.Eq("_id", ObjectId.Parse(id));
            await _collection.ReplaceOneAsync(filter, updatedEntity);
        }
    }
}
