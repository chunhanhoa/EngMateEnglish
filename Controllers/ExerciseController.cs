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

        public ExerciseController(ExerciseRepository exerciseRepository, TopicRepository topicRepository)
        {
            _exerciseRepository = exerciseRepository;
            _topicRepository = topicRepository;
        }

        public async Task<IActionResult> Index()
        {
            var topics = await _topicRepository.GetAllTopicsAsync();
            return View(topics);
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
