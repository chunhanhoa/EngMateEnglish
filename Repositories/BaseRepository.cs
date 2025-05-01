using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using TiengAnh.Models;
using TiengAnh.Services;

namespace TiengAnh.Repositories
{
    public abstract class BaseRepository<T> where T : BaseModel
    {
        protected readonly IMongoCollection<T> _collection;
        
        public BaseRepository(MongoDbService mongoDbService, string collectionName)
        {
            _collection = mongoDbService.GetCollection<T>(collectionName);
        }

        public virtual async Task<List<T>> GetAllAsync()
        {
            return await _collection.Find(_ => true).ToListAsync();
        }

        public virtual async Task<T> GetByIdAsync(string id)
        {
            return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
        }

        public virtual async Task<T> CreateAsync(T entity)
        {
            await _collection.InsertOneAsync(entity);
            return entity;
        }

        public virtual async Task<bool> UpdateAsync(string id, T updatedEntity)
        {
            var result = await _collection.ReplaceOneAsync(x => x.Id == id, updatedEntity);
            return result.ModifiedCount > 0;
        }

        public virtual async Task<bool> DeleteAsync(string id)
        {
            var result = await _collection.DeleteOneAsync(x => x.Id == id);
            return result.DeletedCount > 0;
        }
    }
}
