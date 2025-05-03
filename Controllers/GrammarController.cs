using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TiengAnh.Models;
using TiengAnh.Repositories;
using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace TiengAnh.Controllers
{
    public class GrammarController : Controller
    {
        private readonly ILogger<GrammarController> _logger;
        private readonly GrammarRepository _grammarRepository;

        public GrammarController(
            ILogger<GrammarController> logger,
            GrammarRepository grammarRepository)
        {
            _logger = logger;
            _grammarRepository = grammarRepository;
        }

        public async Task<IActionResult> Index()
        {
            var groupedLessons = await _grammarRepository.GetGroupedGrammarAsync();
            return View(groupedLessons);
        }

        public async Task<IActionResult> Details(int id)
        {
            var grammar = await _grammarRepository.GetByGrammarIdAsync(id);
            
            if (grammar == null)
            {
                return NotFound();
            }

            // Kiểm tra trạng thái yêu thích nếu người dùng đã đăng nhập
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId) && grammar != null)
            {
                grammar.IsFavorite = grammar.FavoriteByUsers != null && 
                                    grammar.FavoriteByUsers.Contains(userId);
            }

            // Ghi log khi người dùng truy cập vào chi tiết bài học
            _logger.LogInformation($"User accessed grammar lesson: {grammar.Title_NP} (ID: {grammar.ID_NP})");
            
            // Đảm bảo có danh sách ví dụ để tránh lỗi null reference
            if (grammar.Examples == null)
            {
                grammar.Examples = new List<string>();
            }
            
            return View(grammar);
        }
        
        public async Task<IActionResult> Level(string level)
        {
            var grammars = await _grammarRepository.GetGrammarsByLevelAsync(level);
            ViewBag.Level = level;
            return View(grammars);
        }

        // Thêm endpoint mới để xử lý yêu thích
        [HttpPost]
        [Authorize] // Yêu cầu đăng nhập
        public async Task<IActionResult> ToggleFavorite(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập để sử dụng tính năng này" });
            }

            var grammar = await _grammarRepository.GetByGrammarIdAsync(id);
            if (grammar == null)
            {
                return Json(new { success = false, message = "Không tìm thấy bài học ngữ pháp" });
            }

            bool isFavorite = false;
            
            // Khởi tạo danh sách nếu chưa có
            if (grammar.FavoriteByUsers == null)
            {
                grammar.FavoriteByUsers = new List<string>();
            }
            
            // Kiểm tra và cập nhật trạng thái yêu thích
            if (grammar.FavoriteByUsers.Contains(userId))
            {
                // Xóa khỏi danh sách yêu thích
                grammar.FavoriteByUsers.Remove(userId);
                isFavorite = false;
            }
            else
            {
                // Thêm vào danh sách yêu thích
                grammar.FavoriteByUsers.Add(userId);
                isFavorite = true;
            }
            
            // Cập nhật vào database
            await _grammarRepository.UpdateAsync(grammar.Id, grammar);
            
            return Json(new 
            { 
                success = true, 
                isFavorite = isFavorite,
                message = isFavorite ? "Đã thêm vào danh sách yêu thích" : "Đã xóa khỏi danh sách yêu thích" 
            });
        }

        [Authorize]
        public async Task<IActionResult> Favorites()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Lấy tất cả grammar point
            var allGrammar = await _grammarRepository.GetAllAsync();
            
            // Lọc ra những grammar point được yêu thích bởi người dùng hiện tại
            var favoriteGrammar = allGrammar
                .Where(g => g.FavoriteByUsers != null && g.FavoriteByUsers.Contains(userId))
                .OrderBy(g => g.Level)
                .ToList();
                
            ViewBag.Title = "Ngữ pháp yêu thích";
            
            return View("FavoriteGrammar", favoriteGrammar);
        }
    }
}
