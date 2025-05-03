using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TiengAnh.Models;
using TiengAnh.Repositories;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace TiengAnh.Controllers
{
    [Authorize] // Đảm bảo chỉ người dùng đăng nhập mới truy cập được
    public class ProgressController : Controller
    {
        private readonly ILogger<ProgressController> _logger;
        private readonly ProgressRepository _progressRepository;
        private readonly VocabularyRepository _vocabularyRepository;
        private readonly GrammarRepository _grammarRepository;
        private readonly TopicRepository _topicRepository;

        public ProgressController(
            ILogger<ProgressController> logger,
            ProgressRepository progressRepository,
            VocabularyRepository vocabularyRepository,
            GrammarRepository grammarRepository,
            TopicRepository topicRepository)
        {
            _logger = logger;
            _progressRepository = progressRepository;
            _vocabularyRepository = vocabularyRepository;
            _grammarRepository = grammarRepository;
            _topicRepository = topicRepository;
        }
        
        public async Task<IActionResult> Index()
        {
            // Lấy ID của người dùng đăng nhập hiện tại từ Claims
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Không thể xác định người dùng đăng nhập");
                return RedirectToAction("Login", "Account");
            }

            // Lấy thông tin tiến độ của người dùng
            var progress = await _progressRepository.GetByUserIdAsync(userId);
            
            // Nếu chưa có dữ liệu tiến độ, tạo mới
            if (progress == null)
            {
                progress = new ProgressModel
                {
                    UserId = userId,
                    VocabularyProgress = 0,
                    GrammarProgress = 0,
                    ExerciseProgress = 0,
                    TotalPoints = 0,
                    Level = "A1", // Mặc định bắt đầu từ A1
                    LastCompletedItems = new List<LastCompletedItemModel>(),
                    CompletedTopics = new List<CompletedTopicModel>()
                };
                
                // Lưu progress mới vào database
                await _progressRepository.AddAsync(progress);
                _logger.LogInformation($"Đã tạo dữ liệu tiến độ mới cho người dùng {userId}");
            }
            
            return View(progress);
        }
        
        public async Task<IActionResult> Favorites()
        {
            // Lấy ID của người dùng đăng nhập hiện tại
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Không thể xác định người dùng đăng nhập");
                return RedirectToAction("Login", "Account");
            }
            
            // Lấy dữ liệu yêu thích của người dùng từ database
            var vocabularies = await _vocabularyRepository.GetFavoriteVocabulariesAsync(userId);
            var grammars = await _grammarRepository.GetFavoriteGrammarsAsync(userId);
            
            ViewBag.Vocabularies = vocabularies;
            ViewBag.Grammars = grammars;
            
            return View();
        }

        // Thêm phương thức để cập nhật tiến độ học
        [HttpPost]
        public async Task<IActionResult> UpdateProgress([FromBody] ProgressUpdateModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            // Lấy ID của người dùng đăng nhập hiện tại
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { Message = "Người dùng chưa đăng nhập" });
            }
            
            // Lấy thông tin tiến độ hiện tại
            var progress = await _progressRepository.GetByUserIdAsync(userId);
            if (progress == null)
            {
                // Tạo mới nếu chưa có
                progress = new ProgressModel
                {
                    UserId = userId,
                    VocabularyProgress = 0,
                    GrammarProgress = 0,
                    ExerciseProgress = 0,
                    TotalPoints = 0,
                    Level = "A1",
                    LastCompletedItems = new List<LastCompletedItemModel>(),
                    CompletedTopics = new List<CompletedTopicModel>()
                };
            }
            
            // Cập nhật thông tin tiến độ
            if (model.Type == "Vocabulary")
            {
                progress.VocabularyProgress = CalculateNewProgress(progress.VocabularyProgress, model.CompletionPercentage);
            }
            else if (model.Type == "Grammar")
            {
                progress.GrammarProgress = CalculateNewProgress(progress.GrammarProgress, model.CompletionPercentage);
            }
            else if (model.Type == "Exercise")
            {
                progress.ExerciseProgress = CalculateNewProgress(progress.ExerciseProgress, model.CompletionPercentage);
            }
            
            // Thêm vào danh sách hoạt động gần đây
            progress.LastCompletedItems.Add(new LastCompletedItemModel
            {
                Id = progress.LastCompletedItems.Count > 0 ? 
                    progress.LastCompletedItems.Max(i => i.Id) + 1 : 1,
                Type = model.Type,
                Title = model.Title,
                CompletedDate = DateTime.Now,
                Score = model.Score
            });
            
            // Giới hạn số lượng hoạt động hiển thị (chỉ giữ 10 hoạt động gần nhất)
            if (progress.LastCompletedItems.Count > 10)
            {
                progress.LastCompletedItems = progress.LastCompletedItems
                    .OrderByDescending(i => i.CompletedDate)
                    .Take(10)
                    .ToList();
            }
            
            // Cập nhật hoặc thêm mới topic nếu có
            if (!string.IsNullOrEmpty(model.TopicName))
            {
                var existingTopic = progress.CompletedTopics
                    .FirstOrDefault(t => t.TopicId == model.TopicId);
                
                if (existingTopic != null)
                {
                    existingTopic.CompletionPercentage = model.CompletionPercentage;
                }
                else
                {
                    progress.CompletedTopics.Add(new CompletedTopicModel
                    {
                        TopicId = model.TopicId,
                        TopicName = model.TopicName,
                        CompletionPercentage = model.CompletionPercentage
                    });
                }
            }
            
            // Cập nhật điểm và cấp độ
            progress.TotalPoints += model.PointsEarned;
            progress.Level = CalculateLevel(progress.TotalPoints);
            
            // Lưu thay đổi vào database
            bool result;
            if (string.IsNullOrEmpty(progress.Id))
            {
                await _progressRepository.AddAsync(progress);
                result = true;
            }
            else
            {
                result = await _progressRepository.UpdateAsync(progress);
            }
            
            if (result)
            {
                return Ok(new { Success = true, Progress = progress });
            }
            
            return StatusCode(500, new { Message = "Không thể cập nhật tiến độ" });
        }
        
        // Phương thức tính toán tiến độ mới
        private int CalculateNewProgress(int currentProgress, int completionPercentage)
        {
            // Tính trung bình có trọng số
            // Ý tưởng: giữ 70% tiến độ hiện tại, thêm 30% từ tiến độ mới
            return (int)Math.Round(currentProgress * 0.7 + completionPercentage * 0.3);
        }
        
        // Phương thức tính toán level dựa trên điểm
        private string CalculateLevel(int points)
        {
            if (points >= 2000) return "C2";
            if (points >= 1500) return "C1";
            if (points >= 1000) return "B2";
            if (points >= 700) return "B1";
            if (points >= 400) return "A2";
            return "A1";
        }

        // Thêm vào mục yêu thích
        [HttpPost]
        public async Task<IActionResult> ToggleFavorite([FromBody] ToggleFavoriteModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            // Lấy ID của người dùng đăng nhập hiện tại
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { Message = "Người dùng chưa đăng nhập" });
            }
            
            bool result = false;
            
            if (model.Type.ToLower() == "vocabulary")
            {
                result = await _vocabularyRepository.ToggleFavoriteAsync(model.Id, userId);
            }
            else if (model.Type.ToLower() == "grammar") 
            {
                result = await _grammarRepository.ToggleFavoriteAsync(model.Id, userId);
            }
            else if (model.Type.ToLower() == "topic")
            {
                // Thêm xử lý cho Topic
                result = await _topicRepository.ToggleFavoriteAsync(model.Id, userId);
            }
            
            return Ok(new { Success = result });
        }

        [HttpPost]
        [Route("Progress/RemoveFavorite/{id}")]
        public async Task<IActionResult> RemoveFavorite(string id, string type)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập để sử dụng tính năng này" });
            }

            if (type == "vocabulary")
            {
                // Xử lý xóa yêu thích từ vựng
                var vocabularyId = int.Parse(id);
                var vocabulary = await _vocabularyRepository.GetByVocabularyIdAsync(vocabularyId);
                
                if (vocabulary == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy từ vựng" });
                }

                // Khởi tạo danh sách nếu chưa có
                if (vocabulary.FavoriteByUsers == null)
                {
                    vocabulary.FavoriteByUsers = new List<string>();
                }
                
                // Xóa khỏi danh sách yêu thích
                if (vocabulary.FavoriteByUsers.Contains(userId))
                {
                    vocabulary.FavoriteByUsers.Remove(userId);
                    await _vocabularyRepository.UpdateAsync(vocabulary.Id, vocabulary);
                }
            }
            else if (type == "grammar")
            {
                // Xử lý xóa yêu thích ngữ pháp
                var grammarId = int.Parse(id);
                var grammar = await _grammarRepository.GetByGrammarIdAsync(grammarId);
                
                if (grammar == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy bài ngữ pháp" });
                }

                // Khởi tạo danh sách nếu chưa có
                if (grammar.FavoriteByUsers == null)
                {
                    grammar.FavoriteByUsers = new List<string>();
                }
                
                // Xóa khỏi danh sách yêu thích
                if (grammar.FavoriteByUsers.Contains(userId))
                {
                    grammar.FavoriteByUsers.Remove(userId);
                    await _grammarRepository.UpdateAsync(grammar.Id, grammar);
                }
            }
            
            return Json(new 
            { 
                success = true,
                message = "Đã xóa khỏi danh sách yêu thích" 
            });
        }
    }

    // Model cho API cập nhật tiến độ
    public class ProgressUpdateModel
    {
        public string Type { get; set; } = string.Empty; // Vocabulary, Grammar hoặc Exercise
        public string Title { get; set; } = string.Empty;
        public int Score { get; set; }
        public int PointsEarned { get; set; }
        public int TopicId { get; set; }
        public string TopicName { get; set; } = string.Empty;
        public int CompletionPercentage { get; set; }
    }

    // Model cho API thêm/xóa yêu thích
    public class ToggleFavoriteModel
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty; // "Vocabulary" hoặc "Grammar"
    }
}
