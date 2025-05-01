using System.Collections.Generic;
using System.Threading.Tasks;
using TiengAnh.Models;

namespace TiengAnh.Repositories
{
    public interface ITopicRepository
    {
        Task<List<TopicModel>> GetAllAsync();
        Task<TopicModel?> GetByIdAsync(string id);
        Task<bool> CreateAsync(TopicModel topic);
        Task<bool> UpdateAsync(string id, TopicModel topic);
        Task<bool> DeleteAsync(string id);
        
        // Thêm các phương thức với tham số int
        Task<List<TopicModel>> GetAllTopicsAsync();
        Task<TopicModel?> GetTopicByIdAsync(int id);
        Task<TopicModel?> GetByTopicIdAsync(int id);
        Task<bool> HasDataAsync();
    }
}
