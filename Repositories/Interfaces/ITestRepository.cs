using System.Collections.Generic;
using System.Threading.Tasks;
using TiengAnh.Models;

namespace TiengAnh.Repositories
{
    public interface ITestRepository
    {
        List<TestModel> GetAllTests();
        Task<List<TestModel>> GetAllTestsAsync();
        Task<TestModel> GetByTestIdAsync(int testId);
        Task<TestModel> GetByStringIdAsync(string id);
        Task<List<TestModel>> GetTestsByCategoryAsync(string category);
        Task<List<TestModel>> GetTestsByLevelAsync(string level);
        Task SeedTestsFromJsonAsync();
        Task<bool> SaveTestAsync(TestModel test);
    }
}
