using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;

namespace TiengAnh.Services
{
    public class MongoDbService
    {
        private readonly IMongoDatabase _database;
        private readonly MongoDbSettings _settings;

        public MongoDbService(IOptions<MongoDbSettings> settings)
        {
            _settings = settings.Value;
            try
            {
                var client = new MongoClient(_settings.ConnectionString);
                _database = client.GetDatabase(_settings.DatabaseName);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to connect to MongoDB: {ex.Message}", ex);
            }
        }

        public IMongoCollection<T> GetCollection<T>(string name)
        {
            try
            {
                return _database.GetCollection<T>(name);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get collection {name}: {ex.Message}", ex);
            }
        }
    }

    public class MongoDbSettings
    {
        public string ConnectionString { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
    }
}
