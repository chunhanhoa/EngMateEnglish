using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Security.Claims;
using TiengAnh.Models;
using TiengAnh.Repositories;

namespace TiengAnh.Controllers
{
    public class VocabularyController : Controller
    {
        private readonly ILogger<VocabularyController> _logger;
        private readonly VocabularyRepository _vocabularyRepository;
        private readonly TopicRepository _topicRepository;

        public VocabularyController(
            ILogger<VocabularyController> logger,
            VocabularyRepository vocabularyRepository,
            TopicRepository topicRepository)
        {
            _logger = logger;
            _vocabularyRepository = vocabularyRepository;
            _topicRepository = topicRepository;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Lấy tất cả các chủ đề và log số lượng
                var allTopics = await _topicRepository.GetAllTopicsAsync();
                _logger.LogInformation("Found {Count} total topics", allTopics.Count);
                
                // Lấy các chủ đề Vocabulary - nếu Type_CD chưa có thì lấy tất cả
                var topics = allTopics.Where(t => string.IsNullOrEmpty(t.Type_CD) || t.Type_CD == "Vocabulary").ToList();
                _logger.LogInformation("Filtered to {Count} vocabulary topics", topics.Count);
                
                // Thêm log để kiểm tra từng topic
                foreach (var topic in topics)
                {
                    _logger.LogInformation("Topic ID: {ID_CD}, Name: {Name_CD}, WordCount: {WordCount}, TotalWords: {TotalWords}", topic.ID_CD, topic.Name_CD, topic.WordCount, topic.TotalWords);
                }
                
                // Trả về view với danh sách chủ đề
                return View(topics);
            }
            catch (System.Exception ex)
            {
                _logger.LogError("Error in Index action: {Message}", ex.Message);
                // Hiển thị lỗi cho người dùng hoặc trả về view với danh sách rỗng
                return View(new List<TopicModel>());
            }
        }

        public async Task<IActionResult> Topic(int id)
        {
            var topic = await _topicRepository.GetByTopicIdAsync(id);
            if (topic == null)
            {
                return NotFound();
            }
            
            var vocabularies = await _vocabularyRepository.GetVocabulariesByTopicIdAsync(id);
            
            // Cập nhật trạng thái yêu thích cho từng từ vựng
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            foreach (var vocab in vocabularies)
            {
                vocab.IsFavorite = !string.IsNullOrEmpty(userId) && vocab.FavoriteByUsers != null && 
                                   vocab.FavoriteByUsers.Contains(userId);
            }
            
            ViewBag.Topic = topic;
            return View(vocabularies);
        }

        public async Task<IActionResult> Details(int id)
        {
            var vocabulary = await _vocabularyRepository.GetByVocabularyIdAsync(id);
            if (vocabulary == null)
            {
                return NotFound();
            }
            
            // Cập nhật trạng thái yêu thích dựa vào người dùng hiện tại
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            vocabulary.IsFavorite = !string.IsNullOrEmpty(userId) && vocabulary.FavoriteByUsers != null && 
                                   vocabulary.FavoriteByUsers.Contains(userId);
            
            var topic = await _topicRepository.GetByTopicIdAsync(vocabulary.ID_CD);
            ViewBag.Topic = topic;
            return View(vocabulary);
        }

        public async Task<IActionResult> Flashcard(int id)
        {
            var topic = await _topicRepository.GetByTopicIdAsync(id);
            if (topic == null)
            {
                return NotFound();
            }
            
            var vocabularies = await _vocabularyRepository.GetVocabulariesByTopicIdAsync(id);
            ViewBag.Topic = topic;
            return View(vocabularies);
        }

        public async Task<IActionResult> Exercise(int id)
        {
            var topic = await _topicRepository.GetByTopicIdAsync(id);
            if (topic == null)
            {
                return NotFound();
            }
            
            var vocabularies = await _vocabularyRepository.GetVocabulariesByTopicIdAsync(id);
            ViewBag.Topic = topic;
            return View(vocabularies);
        }

        // Thêm phương thức để khởi tạo dữ liệu chủ đề

        public async Task<IActionResult> InitializeTopics()
        {
            if (await _topicRepository.HasDataAsync())
            {
                return RedirectToAction("Index", new { message = "Data already exists" });
            }

            var topics = new List<TopicModel>
            {
                new TopicModel
                {
                    ID_CD = 1,
                    Name_CD = "Animals",
                    Description_CD = "Từ vựng về các loài động vật phổ biến",
                    IconPath = "/images/topics/animals.png",
                    Image_CD = "/images/topics/animals.jpg",
                    Level = "A1",
                    TotalItems = 15,
                    TotalWords = 15,
                    WordCount = 15,
                    BackgroundColor = "#f8d7da",
                    Type_CD = "Vocabulary"
                },
                new TopicModel
                {
                    ID_CD = 2,
                    Name_CD = "Food & Drinks",
                    Description_CD = "Từ vựng về đồ ăn và thức uống",
                    IconPath = "/images/topics/food.png",
                    Image_CD = "/images/topics/food.jpg",
                    Level = "A1",
                    TotalItems = 20,
                    TotalWords = 20,
                    WordCount = 20,
                    BackgroundColor = "#d1e7dd",
                    Type_CD = "Vocabulary"
                },
                new TopicModel
                {
                    ID_CD = 3,
                    Name_CD = "School",
                    Description_CD = "Từ vựng về trường học và học tập",
                    IconPath = "/images/topics/school.png",
                    Image_CD = "/images/topics/school.jpg",
                    Level = "A1",
                    TotalItems = 18,
                    TotalWords = 18,
                    WordCount = 18,
                    BackgroundColor = "#cff4fc",
                    Type_CD = "Vocabulary"
                }
            };

            foreach (var topic in topics)
            {
                await _topicRepository.CreateAsync(topic);
            }

            return RedirectToAction("Index", new { message = "Initialized topics" });
        }
    }
}
