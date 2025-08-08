using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using TiengAnh.Models;
using TiengAnh.Services;
using System.IO;
using System.Text.Json;
using System.Linq;

namespace TiengAnh.Repositories
{
    public class ExerciseRepository : BaseRepository<ExerciseModel>
    {
        private readonly string _jsonPath;
        private List<ExerciseModel> _cachedExercises;

        public ExerciseRepository(MongoDbService mongoDbService, string contentRootPath) : base(mongoDbService, "Exercises")
        {
            _jsonPath = Path.Combine(contentRootPath, "json", "exercise.json");
            LoadExercisesFromJson();
        }

        private void LoadExercisesFromJson()
        {
            if (File.Exists(_jsonPath))
            {
                string jsonContent = File.ReadAllText(_jsonPath);
                _cachedExercises = JsonSerializer.Deserialize<List<ExerciseModel>>(
                    jsonContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
            }
            else
            {
                _cachedExercises = new List<ExerciseModel>();
            }
        }

        public async Task<List<ExerciseModel>> GetExercisesByTopicIdAsync(int topicId)
        {
            // Try to get from database first
            var filter = Builders<ExerciseModel>.Filter.Eq(x => x.ID_CD, topicId);
            var exercisesFromDb = await _collection.Find(filter).ToListAsync();
            
            if (exercisesFromDb != null && exercisesFromDb.Count > 0)
            {
                return exercisesFromDb;
            }
            
            // If no exercises in database, use the JSON file
            return _cachedExercises?.Where(e => e.ID_CD == topicId).ToList() ?? new List<ExerciseModel>();
        }

        public async Task<ExerciseModel> GetByExerciseIdAsync(int exerciseId)
        {
            // Try to get from database first
            var filter = Builders<ExerciseModel>.Filter.Eq(x => x.ID_BT, exerciseId);
            var exerciseFromDb = await _collection.Find(filter).FirstOrDefaultAsync();
            
            if (exerciseFromDb != null)
            {
                return exerciseFromDb;
            }
            
            // If not found in database, use the JSON file
            return _cachedExercises?.FirstOrDefault(e => e.ID_BT == exerciseId);
        }
        
        // Method to ensure data is loaded into MongoDB from JSON file
        public async Task SeedExercisesFromJsonAsync()
        {
            if (!File.Exists(_jsonPath))
            {
                return;
            }

            // Check if collection is empty
            var count = await _collection.CountDocumentsAsync(FilterDefinition<ExerciseModel>.Empty);
            if (count > 0)
            {
                return; // Data already exists
            }

            // Load exercises from JSON and insert into MongoDB
            string jsonContent = File.ReadAllText(_jsonPath);
            var exercises = JsonSerializer.Deserialize<List<ExerciseModel>>(
                jsonContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (exercises != null && exercises.Count > 0)
            {
                await _collection.InsertManyAsync(exercises);
            }
        }

        // Tạo ID_BT tiếp theo (max + 1)
        public async Task<int> GetNextIdAsync()
        {
            try
            {
                var max = await _collection.Find(_ => true)
                    .SortByDescending(x => x.ID_BT)
                    .Limit(1)
                    .FirstOrDefaultAsync();
                return max != null ? (max.ID_BT + 1) : 1;
            }
            catch
            {
                return 1;
            }
        }

        // Xóa theo ID_BT
        public async Task<bool> DeleteByExerciseIdAsync(int exerciseId)
        {
            var result = await _collection.DeleteOneAsync(x => x.ID_BT == exerciseId);
            return result.DeletedCount > 0;
        }
    }
}
