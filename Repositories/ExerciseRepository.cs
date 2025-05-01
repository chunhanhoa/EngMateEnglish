using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using TiengAnh.Models;
using TiengAnh.Services;

namespace TiengAnh.Repositories
{
    public class ExerciseRepository : BaseRepository<ExerciseModel>
    {
        public ExerciseRepository(MongoDbService mongoDbService) : base(mongoDbService, "Exercise")
        {
        }

        public async Task<ExerciseModel> GetExerciseByIdAsync(int id)
        {
            return await _collection.Find(x => x.ID_BT == id).FirstOrDefaultAsync();
        }

        public async Task<List<ExerciseModel>> GetExercisesByTopicIdAsync(int topicId)
        {
            return await _collection.Find(x => x.ID_CD == topicId).ToListAsync();
        }

        public async Task<List<ExerciseModel>> GetExercisesByTopicAndTypeAsync(int topicId, string exerciseType)
        {
            return await _collection.Find(x => x.ID_CD == topicId && x.ExerciseType == exerciseType).ToListAsync();
        }
    }
}
