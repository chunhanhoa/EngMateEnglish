using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiengAnh.Data;
using TiengAnh.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TiengAnh.Controllers
{
    public class ExerciseController : Controller
    {
        private readonly ILogger<ExerciseController> _logger;
        private readonly HocTiengAnhContext _context;

        public ExerciseController(ILogger<ExerciseController> logger, HocTiengAnhContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Chuyển đổi ChuDe sang TopicModel
            var chuDes = await _context.ChuDes.ToListAsync();
            var topics = new List<TopicModel>();
            
            foreach (var chuDe in chuDes)
            {
                var exerciseCount = await _context.BaiTaps.CountAsync(b => b.IdCd == chuDe.IdCd);
                
                topics.Add(new TopicModel
                {
                    ID_CD = chuDe.IdCd,
                    Name_CD = chuDe.NameCd ?? string.Empty,
                    Description_CD = chuDe.DiscriptionCd ?? string.Empty,
                    IconPath = "/images/icons/topic-default.png",
                    BackgroundColor = "#e6f6ff",
                    WordCount = exerciseCount // Sử dụng WordCount cho số lượng bài tập
                });
            }
            
            return View(topics);
        }

        public async Task<IActionResult> Topic(int id)
        {
            var exercises = await _context.BaiTaps
                .Where(e => e.IdCd == id)
                .ToListAsync();
                
            var topic = await _context.ChuDes.FindAsync(id);
            
            if (topic == null)
            {
                return NotFound();
            }

            ViewBag.Topic = topic;
            return View(exercises);
        }
        
        // Các action mới cho các loại bài tập
        public async Task<IActionResult> MultipleChoice(int id)
        {
            var exercises = await _context.BaiTaps
                .Where(e => e.IdCd == id)
                .ToListAsync();
                
            var topic = await _context.ChuDes.FindAsync(id);
            
            if (topic == null)
            {
                return NotFound();
            }

            ViewBag.Topic = topic;
            return View(exercises);
        }
        
        public async Task<IActionResult> FillBlank(int id)
        {
            var exercises = await _context.BaiTaps
                .Where(e => e.IdCd == id)
                .ToListAsync();
                
            var topic = await _context.ChuDes.FindAsync(id);
            
            if (topic == null)
            {
                return NotFound();
            }

            // Chuẩn bị câu hỏi dạng điền từ
            foreach (var exercise in exercises)
            {
                // Thay đổi câu hỏi để có định dạng điền từ
                exercise.QuestionBt = CreateFillBlankQuestion(exercise);
            }

            ViewBag.Topic = topic;
            return View(exercises);
        }
        
        public async Task<IActionResult> WordOrdering(int id)
        {
            var exercises = await _context.BaiTaps
                .Where(e => e.IdCd == id)
                .ToListAsync();
                
            var topic = await _context.ChuDes.FindAsync(id);
            
            if (topic == null)
            {
                return NotFound();
            }

            // Chuẩn bị câu hỏi dạng sắp xếp từ
            foreach (var exercise in exercises)
            {
                // Tạo câu hoàn chỉnh cho bài tập sắp xếp từ
                exercise.QuestionBt = CreateWordOrderQuestion(exercise);
            }

            ViewBag.Topic = topic;
            return View(exercises);
        }

        // Action để hiển thị trang bài tập cho ngữ pháp cụ thể
        public async Task<IActionResult> GrammarExercise(int id)
        {
            try
            {
                // Tìm bài ngữ pháp theo ID
                var nguPhap = await _context.NguPhaps
                    .Include(n => n.IdCdNavigation)
                    .FirstOrDefaultAsync(n => n.IdNp == id);
                    
                if (nguPhap == null)
                {
                    _logger.LogWarning($"Không tìm thấy ngữ pháp với ID = {id}");
                    return NotFound();
                }
                
                // Tìm các bài tập thuộc chủ đề của bài ngữ pháp này
                var exercises = await _context.BaiTaps
                    .Where(b => b.IdCd == nguPhap.IdCd)
                    .Take(10) // Giới hạn số lượng bài tập
                    .ToListAsync();
                    
                _logger.LogInformation($"Tìm thấy {exercises.Count} bài tập cho ngữ pháp ID = {id}");
                
                // Chuyển đổi dữ liệu để truyền vào view
                var grammarModel = new GrammarModel
                {
                    ID_NP = nguPhap.IdNp,
                    Title_NP = nguPhap.TitleNp ?? string.Empty,
                    Description_NP = nguPhap.DiscriptionNp ?? string.Empty,
                    TimeUpload_NP = nguPhap.TimeuploadNp.HasValue ? 
                        new DateTime(nguPhap.TimeuploadNp.Value.Year, nguPhap.TimeuploadNp.Value.Month, nguPhap.TimeuploadNp.Value.Day) : null,
                    Content = nguPhap.ContentNp ?? string.Empty,
                    ID_CD = nguPhap.IdCd,
                    TopicName = nguPhap.IdCdNavigation?.NameCd ?? string.Empty
                };
                
                ViewBag.Grammar = grammarModel;
                ViewBag.Topic = nguPhap.IdCdNavigation;
                
                return View(exercises);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi tải bài tập ngữ pháp ID = {id}");
                return RedirectToAction("Index", "Grammar");
            }
        }
        
        // Action để lấy các bài tập động qua AJAX
        public async Task<IActionResult> GetGrammarExercises(int grammarId, string type)
        {
            // Tìm bài ngữ pháp
            var nguPhap = await _context.NguPhaps
                .FirstOrDefaultAsync(n => n.IdNp == grammarId);
                
            if (nguPhap == null)
            {
                return PartialView("_NoExercises");
            }
            
            // Tìm các bài tập thuộc chủ đề của bài ngữ pháp này
            var exercises = await _context.BaiTaps
                .Where(b => b.IdCd == nguPhap.IdCd)
                .Take(5) // Giới hạn số lượng bài tập
                .ToListAsync();
                
            if (!exercises.Any())
            {
                return PartialView("_NoExercises");
            }
            
            // Trả về partial view tương ứng với loại bài tập
            switch (type)
            {
                case "multiple-choice":
                    return PartialView("_MultipleChoiceExercises", exercises);
                
                case "fill-blank":
                    return PartialView("_FillBlankExercises", exercises);
                
                case "word-order":
                    return PartialView("_WordOrderExercises", exercises);
                
                default:
                    return PartialView("_MultipleChoiceExercises", exercises);
            }
        }

        [HttpPost]
        public ActionResult SaveProgress(int grammarId, int score, int totalQuestions)
        {
            try
            {
                // Mã này sẽ thực hiện lưu tiến trình học tập của người dùng
                // Tạm thời chỉ return success
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lưu tiến trình bài tập");
                return Json(new { success = false });
            }
        }

        private string CreateFillBlankQuestion(BaiTap exercise)
        {
            // Giả định câu hỏi gốc không có [BLANK], thêm vào để tạo dạng fill in the blank
            var questionText = exercise.QuestionBt ?? string.Empty;
            
            if (!questionText.Contains("[BLANK]"))
            {
                // Tìm một từ để thay thế bằng [BLANK]
                var words = questionText.Split(' ');
                if (words.Length > 3)
                {
                    // Chọn một từ ở giữa câu để thay thế
                    var wordToReplace = words[words.Length / 2];
                    questionText = questionText.Replace(wordToReplace, "[BLANK]");
                }
                else
                {
                    // Nếu câu quá ngắn thì thêm [BLANK] vào cuối
                    questionText += " [BLANK]";
                }
            }
            
            return questionText;
        }

        private string CreateWordOrderQuestion(BaiTap exercise)
        {
            // Tạo câu hoàn chỉnh cho bài tập sắp xếp từ
            return exercise.QuestionBt ?? "Sắp xếp các từ để tạo câu hoàn chỉnh.";
        }
    }
}
