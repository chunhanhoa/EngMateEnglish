using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TiengAnh.Models;
using TiengAnh.Repositories;
using TiengAnh.Services;

namespace TiengAnh.Controllers
{
    public class ExerciseController : Controller
    {
        private readonly ExerciseRepository _exerciseRepository;
        private readonly TopicRepository _topicRepository;
        private readonly ILogger<ExerciseController> _logger;

        public ExerciseController(
            ExerciseRepository exerciseRepository, 
            TopicRepository topicRepository,
            ILogger<ExerciseController> logger)
        {
            _exerciseRepository = exerciseRepository;
            _topicRepository = topicRepository;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var topics = await _topicRepository.GetAllTopicsAsync();
                
                // For each topic, get the exercises count
                foreach (var topic in topics)
                {
                    var exercises = await _exerciseRepository.GetExercisesByTopicIdAsync(topic.ID_CD);
                    topic.ExerciseCount = exercises.Count;
                    
                    // Count by exercise type
                    topic.MultipleChoiceCount = exercises.Count(e => e.ExerciseType == "MultipleChoice");
                    topic.FillBlankCount = exercises.Count(e => e.ExerciseType == "FillBlank");
                    topic.WordOrderingCount = exercises.Count(e => e.ExerciseType == "WordOrdering");
                    
                    _logger.LogInformation($"Topic {topic.ID_CD} ({topic.Name_CD}): {topic.ExerciseCount} exercises " +
                        $"(MC: {topic.MultipleChoiceCount}, FB: {topic.FillBlankCount}, WO: {topic.WordOrderingCount})");
                }
                
                return View(topics);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error retrieving topics and exercises for Index view");
                return View(new List<TopicModel>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Topic(int id)
        {
            // Get topic information
            var topic = await _topicRepository.GetTopicByIdAsync(id);
            if (topic == null)
            {
                return NotFound();
            }

            // Get exercises for this topic
            var exercises = await _exerciseRepository.GetExercisesByTopicIdAsync(id);
            
            ViewBag.Topic = topic;
            
            return View(exercises);
        }

        [HttpGet]
        public async Task<IActionResult> MultipleChoice(int id)
        {
            var exercise = await _exerciseRepository.GetByExerciseIdAsync(id);
            if (exercise == null)
            {
                return NotFound();
            }
            
            // Get topic information for breadcrumbs
            var topic = await _topicRepository.GetTopicByIdAsync(exercise.ID_CD);
            ViewBag.Topic = topic;
            
            return View(exercise);
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
            var exercises = await _exerciseRepository.GetExercisesByTopicIdAsync(id);
            // Lọc theo loại bài tập (nếu có)
            var fillBlankExercises = exercises.Where(e => e.ExerciseType == "FillBlank").ToList();
            
            if (fillBlankExercises.Count == 0)
            {
                // Nếu không có bài tập loại này, sử dụng tất cả bài tập của chủ đề
                fillBlankExercises = exercises;
            }
            
            ViewBag.Topic = topic;
            return View(fillBlankExercises);
        }

        // Phương thức xử lý bài tập sắp xếp câu
        public async Task<IActionResult> WordOrdering(int id)
        {
            var topic = await _topicRepository.GetTopicByIdAsync(id);
            if (topic == null)
            {
                return NotFound();
            }

            // Lấy các bài tập từ cơ sở dữ liệu
            var exercises = await _exerciseRepository.GetExercisesByTopicIdAsync(id);
            // Lọc theo loại bài tập (nếu có)
            var wordOrderingExercises = exercises.Where(e => e.ExerciseType == "WordOrdering").ToList();
            
            if (wordOrderingExercises.Count == 0)
            {
                // Nếu không có bài tập loại này, sử dụng tất cả bài tập của chủ đề
                wordOrderingExercises = exercises;
            }
            
            ViewBag.Topic = topic;
            return View(wordOrderingExercises);
        }
    }
}
