using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TiengAnh.Models;
using TiengAnh.Services;

namespace TiengAnh.Repositories
{
    public class TestRepository : BaseRepository<TestModel>, ITestRepository
    {
        private readonly IMongoCollection<TestModel> _collection;
        private List<TestModel> _cachedTests;

        public TestRepository(MongoDbService mongoDbService) : base(mongoDbService, "Tests")
        {
            _collection = mongoDbService.GetCollection<TestModel>("Tests");
            _cachedTests = new List<TestModel>();
            Task.Run(async () => await SeedTestsFromJsonAsync()).Wait();
        }

        // Implement missing interface methods
        public List<TestModel> GetAllTests()
        {
            return _cachedTests ?? _collection.Find(_ => true).ToList();
        }

        public override async Task<List<TestModel>> GetAllAsync()
        {
            if (_cachedTests != null && _cachedTests.Count > 0)
            {
                return _cachedTests;
            }

            return await _collection.Find(_ => true).ToListAsync();
        }

        public async Task<List<TestModel>> GetAllTestsAsync()
        {
            return await GetAllAsync();
        }

        public async Task<TestModel> GetByTestIdAsync(int testId)
        {
            // First try to find in the database
            var filter = Builders<TestModel>.Filter.Eq("TestId", testId);
            var test = await _collection.Find(filter).FirstOrDefaultAsync();

            // If not found in database, check cached tests
            if (test == null && _cachedTests != null)
            {
                test = _cachedTests.FirstOrDefault(t => t.Id == testId.ToString() || 
                                                     t.TestIdentifier == testId.ToString());
            }

            return test;
        }

        public async Task<TestModel> GetByStringIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }
            
            try
            {
                // First try to find in the database by TestIdentifier
                var filter = Builders<TestModel>.Filter.Eq(x => x.TestIdentifier, id);
                var test = await _collection.Find(filter).FirstOrDefaultAsync();
                
                // If not found, try by JsonId
                if (test == null)
                {
                    filter = Builders<TestModel>.Filter.Eq(x => x.JsonId, id);
                    test = await _collection.Find(filter).FirstOrDefaultAsync();
                }
                
                // If still not found, try by Id
                if (test == null)
                {
                    filter = Builders<TestModel>.Filter.Eq(x => x.Id, id);
                    test = await _collection.Find(filter).FirstOrDefaultAsync();
                }
                
                // If not found in database, check cached tests
                if (test == null)
                {
                    test = _cachedTests?.FirstOrDefault(t => 
                        t.TestIdentifier == id || 
                        t.JsonId == id || 
                        t.Id == id);
                }
                
                return test;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetByStringIdAsync: {ex.Message}");
                // Fall back to cached tests
                return _cachedTests?.FirstOrDefault(t => 
                    t.TestIdentifier == id || 
                    t.JsonId == id || 
                    t.Id == id);
            }
        }

        public async Task<List<TestModel>> GetTestsByCategoryAsync(string category)
        {
            var filter = Builders<TestModel>.Filter.Eq(t => t.Category, category);
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<List<TestModel>> GetTestsByLevelAsync(string level)
        {
            var filter = Builders<TestModel>.Filter.Eq(t => t.Level, level);
            return await _collection.Find(filter).ToListAsync();
        }

        // Change the visibility from private to public to match the interface
        public async Task SeedTestsFromJsonAsync()
        {
            try
            {
                var count = await _collection.CountDocumentsAsync(FilterDefinition<TestModel>.Empty);
                
                if (count == 0)
                {
                    // Load from JSON
                    string jsonFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "tests.json");
                    
                    if (File.Exists(jsonFilePath))
                    {
                        string jsonContent = await File.ReadAllTextAsync(jsonFilePath);
                        _cachedTests = JsonSerializer.Deserialize<List<TestModel>>(jsonContent);
                        
                        if (_cachedTests != null)
                        {
                            // Generate TestIdentifier if missing
                            int i = 1;
                            foreach (var test in _cachedTests)
                            {
                                if (string.IsNullOrEmpty(test.TestIdentifier))
                                {
                                    if (!string.IsNullOrEmpty(test.JsonId))
                                    {
                                        test.TestIdentifier = test.JsonId;
                                    }
                                    else
                                    {
                                        test.TestIdentifier = $"test_{i:D3}";
                                    }
                                }
                                i++;
                            }
                            
                            // Insert to database
                            await _collection.InsertManyAsync(_cachedTests);
                        }
                    }
                }
                else
                {
                    // Load tests from database
                    _cachedTests = await _collection.Find(_ => true).ToListAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding tests: {ex.Message}");
            }
        }
        
        // Implement the SaveTestAsync method
        public async Task<bool> SaveTestAsync(TestModel test)
        {
            try
            {
                if (string.IsNullOrEmpty(test.Id))
                {
                    // This is a new test
                    await _collection.InsertOneAsync(test);
                    
                    // Update cache
                    if (_cachedTests != null)
                    {
                        _cachedTests.Add(test);
                    }
                    
                    return true;
                }
                else
                {
                    // This is an existing test
                    var filter = Builders<TestModel>.Filter.Eq(x => x.Id, test.Id);
                    var result = await _collection.ReplaceOneAsync(filter, test);
                    
                    // Update cache
                    if (_cachedTests != null)
                    {
                        var existingIndex = _cachedTests.FindIndex(t => t.Id == test.Id);
                        if (existingIndex >= 0)
                        {
                            _cachedTests[existingIndex] = test;
                        }
                    }
                    
                    return result.IsAcknowledged && result.ModifiedCount > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving test: {ex.Message}");
                return false;
            }
        }
    }
}