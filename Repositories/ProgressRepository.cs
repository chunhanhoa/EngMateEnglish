using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using TiengAnh.Models;
using TiengAnh.Services;

namespace TiengAnh.Repositories
{
    public class ProgressRepository : BaseRepository<ProgressModel>
    {
        public ProgressRepository(MongoDbService mongoDbService) : base(mongoDbService, "Progress")
        {
        }

        public async Task<ProgressModel> GetByUserIdAsync(string userId)
        {
            return await _collection.Find(x => x.UserId == userId).FirstOrDefaultAsync();
        }

        // Thêm phương thức để thêm mới tiến độ
        public async Task<bool> AddAsync(ProgressModel progress)
        {
            try
            {
                await _collection.InsertOneAsync(progress);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Thêm phương thức để cập nhật tiến độ
        public async Task<bool> UpdateAsync(ProgressModel progress)
        {
            try
            {
                var filter = Builders<ProgressModel>.Filter.Eq(x => x.Id, progress.Id);
                var update = Builders<ProgressModel>.Update
                    .Set(x => x.VocabularyProgress, progress.VocabularyProgress)
                    .Set(x => x.GrammarProgress, progress.GrammarProgress)
                    .Set(x => x.ExerciseProgress, progress.ExerciseProgress)
                    .Set(x => x.TotalPoints, progress.TotalPoints)
                    .Set(x => x.Level, progress.Level)
                    .Set(x => x.LastCompletedItems, progress.LastCompletedItems)
                    .Set(x => x.CompletedTopics, progress.CompletedTopics);

                var result = await _collection.UpdateOneAsync(filter, update);
                return result.ModifiedCount > 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
