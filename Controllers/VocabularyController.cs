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
            // Lấy danh sách tất cả các chủ đề
            var topics = await _topicRepository.GetAllTopicsAsync();
            
            // Lấy danh sách tất cả từ vựng để đếm
            var allVocabularies = await _vocabularyRepository.GetAllAsync();
            
            // Cập nhật số lượng từ cho mỗi chủ đề
            foreach (var topic in topics)
            {
                int wordCount = allVocabularies.Count(v => v.ID_CD == topic.ID_CD);
                topic.WordCount = wordCount;
                topic.TotalItems = wordCount;
            }
            
            // Lấy ID người dùng hiện tại từ claims nếu đã đăng nhập
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            
            if (!string.IsNullOrEmpty(userId))
            {
                // Kiểm tra và thiết lập trạng thái yêu thích cho từng chủ đề
                foreach (var topic in topics)
                {
                    // Chỉ cập nhật khi người dùng đã đăng nhập
                    if (topic.FavoriteByUsers != null && topic.FavoriteByUsers.Contains(userId))
                    {
                        topic.IsFavorite = true;
                    }
                }
            }
            
            return View(topics);
        }

        public async Task<IActionResult> Topic(int id, int page = 1)
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
            ViewBag.Page = page;
            
            return View(vocabularies);
        }

        public async Task<IActionResult> Details(int id)
        {
            var vocabulary = await _vocabularyRepository.GetByVocabularyIdAsync(id);
            if (vocabulary == null)
            {
                return NotFound();
            }
            
            // Lấy danh sách các từ vựng cùng chủ đề
            var relatedVocabularies = await _vocabularyRepository.GetByTopicIdAsync(vocabulary.ID_CD);
            
            // Loại bỏ từ vựng hiện tại khỏi danh sách
            relatedVocabularies = relatedVocabularies.Where(v => v.ID_TV != vocabulary.ID_TV).ToList();
            
            // Random lấy tối đa 5 từ vựng liên quan
            var randomRelatedWords = relatedVocabularies
                .OrderBy(v => Guid.NewGuid()) // Sắp xếp ngẫu nhiên
                .Take(5)
                .ToList();
            
            // Truyền danh sách từ vựng liên quan qua ViewBag
            ViewBag.RelatedWords = randomRelatedWords;
            
            // Cập nhật trạng thái yêu thích nếu người dùng đã đăng nhập
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                vocabulary.IsFavorite = vocabulary.IsFavoriteByUser(userId);
            }
            
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
        
        [HttpPost]
        [Route("Vocabulary/ToggleFavorite/{id}")]
        public async Task<IActionResult> ToggleFavorite(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập để sử dụng tính năng này" });
            }

            var vocabulary = await _vocabularyRepository.GetByVocabularyIdAsync(id);
            if (vocabulary == null)
            {
                return Json(new { success = false, message = "Không tìm thấy từ vựng" });
            }

            bool isFavorite = false;
            
            // Khởi tạo danh sách nếu chưa có
            if (vocabulary.FavoriteByUsers == null)
            {
                vocabulary.FavoriteByUsers = new List<string>();
            }
            
            // Kiểm tra và cập nhật trạng thái yêu thích
            if (vocabulary.FavoriteByUsers.Contains(userId))
            {
                // Xóa khỏi danh sách yêu thích
                vocabulary.FavoriteByUsers.Remove(userId);
                isFavorite = false;
            }
            else
            {
                // Thêm vào danh sách yêu thích
                vocabulary.FavoriteByUsers.Add(userId);
                isFavorite = true;
            }
            
            // Cập nhật vào database
            await _vocabularyRepository.UpdateAsync(vocabulary.Id, vocabulary);
            
            return Json(new 
            { 
                success = true, 
                isFavorite = isFavorite,
                message = isFavorite ? "Đã thêm vào danh sách yêu thích" : "Đã xóa khỏi danh sách yêu thích" 
            });
        }
    }
}
