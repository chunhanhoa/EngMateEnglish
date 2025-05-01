using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using TiengAnh.Models;
using TiengAnh.Services;

namespace TiengAnh.Repositories
{
    public class TestRepository : BaseRepository<TestModel>
    {
        public TestRepository(MongoDbService mongoDbService) : base(mongoDbService, "Test")
        {
        }

        public async Task<TestModel> GetByTestIdAsync(int testId)
        {
            return await _collection.Find(x => x.TestID == testId).FirstOrDefaultAsync();
        }

        public async Task<List<TestModel>> GetSampleTestsAsync()
        {
            // Trong một ứng dụng thực tế, đây có thể là một truy vấn phức tạp hơn
            // để lấy ra các bài kiểm tra mẫu dựa trên tiêu chí khác nhau
            return await _collection.Find(_ => true).ToListAsync();
        }
    }
}
