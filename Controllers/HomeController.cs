using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TiengAnh.Models;
using TiengAnh.Repositories;
using System.Linq;
using TiengAnh.Services; // Thêm import này cho MongoDbService

namespace TiengAnh.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ITopicRepository _topicRepository;

        public HomeController(MongoDbService mongoDbService, ILogger<HomeController> logger, ITopicRepository topicRepository) 
            : base(mongoDbService)
        {
            _logger = logger;
            _topicRepository = topicRepository;
        }

        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Tải dữ liệu chủ đề cho trang chủ");
            var topics = await _topicRepository.GetAllAsync();
            
            // Chỉ hiển thị 6 chủ đề đầu tiên trên trang chủ
            var featuredTopics = topics.Take(6).ToList();
            
            ViewBag.Topics = featuredTopics;
            
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
