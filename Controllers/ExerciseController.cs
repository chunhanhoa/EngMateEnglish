using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using TiengAnh.Models;
using TiengAnh.Repositories;

namespace TiengAnh.Controllers
{
    public class ExerciseController : Controller
    {
        private readonly ILogger<ExerciseController> _logger;
        private readonly ExerciseRepository _exerciseRepository;
        private readonly TopicRepository _topicRepository;

        public ExerciseController(
            ILogger<ExerciseController> logger,
            ExerciseRepository exerciseRepository,
            TopicRepository topicRepository)
        {
            _logger = logger;
            _exerciseRepository = exerciseRepository;
            _topicRepository = topicRepository;
        }

        public async Task<IActionResult> Index()
        {
            var topics = await _topicRepository.GetAllTopicsAsync();
            _logger.LogInformation("Đã tìm thấy {Count} chủ đề từ database", topics.Count);
            return View(topics);
        }

        public async Task<IActionResult> Topic(int id)
        {
            var topic = await _topicRepository.GetTopicByIdAsync(id);
            if (topic == null)
            {
                return NotFound();
            }

            var exercises = await _exerciseRepository.GetExercisesByTopicIdAsync(id);
            ViewBag.Topic = topic;
            return View(exercises);
        }

        // Phương thức xử lý bài tập trắc nghiệm
        public async Task<IActionResult> MultipleChoice(int id)
        {
            var topic = await _topicRepository.GetTopicByIdAsync(id);
            if (topic == null)
            {
                return NotFound();
            }

            // Lấy các bài tập trắc nghiệm từ cơ sở dữ liệu
            var exercises = await _exerciseRepository.GetExercisesByTopicAndTypeAsync(id, "MultipleChoice");
            if (exercises.Count == 0)
            {
                // Nếu không có bài tập loại này, lấy tất cả bài tập của chủ đề
                exercises = await _exerciseRepository.GetExercisesByTopicIdAsync(id);
                _logger.LogWarning("Không tìm thấy bài tập trắc nghiệm cho chủ đề {Id}, hiển thị tất cả bài tập", id);
            }
            
            ViewBag.Topic = topic;
            return View(exercises);
        }

        // Phương thức xử lý bài tập điền từ
        public async Task<IActionResult> FillBlank(int id)
        {
            var topic = await _topicRepository.GetTopicByIdAsync(id);
            if (topic == null)
            {
                return NotFound();
            }

            // Lấy các bài tập điền từ từ cơ sở dữ liệu
            var exercises = await _exerciseRepository.GetExercisesByTopicAndTypeAsync(id, "FillBlank");
            if (exercises.Count == 0)
            {
                // Nếu không có bài tập loại này, lấy tất cả bài tập của chủ đề
                exercises = await _exerciseRepository.GetExercisesByTopicIdAsync(id);
                _logger.LogWarning("Không tìm thấy bài tập điền từ cho chủ đề {Id}, hiển thị tất cả bài tập", id);
            }
            
            ViewBag.Topic = topic;
            return View(exercises);
        }

        // Phương thức xử lý bài tập sắp xếp câu
        public async Task<IActionResult> WordOrdering(int id)
        {
            var topic = await _topicRepository.GetTopicByIdAsync(id);
            if (topic == null)
            {
                return NotFound();
            }

            // Lấy các bài tập sắp xếp câu từ cơ sở dữ liệu
            var exercises = await _exerciseRepository.GetExercisesByTopicAndTypeAsync(id, "WordOrdering");
            if (exercises.Count == 0)
            {
                // Nếu không có bài tập loại này, lấy tất cả bài tập của chủ đề
                exercises = await _exerciseRepository.GetExercisesByTopicIdAsync(id);
                _logger.LogWarning("Không tìm thấy bài tập sắp xếp câu cho chủ đề {Id}, hiển thị tất cả bài tập", id);
            }
            
            ViewBag.Topic = topic;
            return View(exercises);
        }
    }
}
