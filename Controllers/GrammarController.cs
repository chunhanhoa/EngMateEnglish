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
    public class GrammarController : Controller
    {
        private readonly ILogger<GrammarController> _logger;
        private readonly GrammarRepository _grammarRepository;

        public GrammarController(ILogger<GrammarController> logger, GrammarRepository grammarRepository)
        {
            _logger = logger;
            _grammarRepository = grammarRepository;
        }
        
        public async Task<IActionResult> Index()
        {
            // Lấy ID người dùng hiện tại từ claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            
            var grammarDict = await _grammarRepository.GetGrammarsByLevelGroupAsync();
            
            // Cập nhật trạng thái yêu thích cho từng bài ngữ pháp
            foreach (var level in grammarDict.Keys)
            {
                foreach (var grammar in grammarDict[level])
                {
                    grammar.IsFavorite = !string.IsNullOrEmpty(userId) && grammar.FavoriteByUsers != null && 
                                         grammar.FavoriteByUsers.Contains(userId);
                }
            }
            
            return View(grammarDict);
        }
        
        public async Task<IActionResult> Level(string level)
        {
            // Lấy ID người dùng hiện tại từ claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            
            var grammars = await _grammarRepository.GetGrammarsByLevelAsync(level);
            
            // Cập nhật trạng thái yêu thích cho từng bài ngữ pháp
            foreach (var grammar in grammars)
            {
                grammar.IsFavorite = !string.IsNullOrEmpty(userId) && grammar.FavoriteByUsers != null && 
                                     grammar.FavoriteByUsers.Contains(userId);
            }
            
            ViewBag.Level = level;
            return View(grammars);
        }

        public async Task<IActionResult> Details(int id)
        {
            var grammar = await _grammarRepository.GetByGrammarIdAsync(id);
            if (grammar == null)
            {
                return NotFound();
            }
            
            // Cập nhật trạng thái yêu thích dựa vào người dùng hiện tại
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            grammar.IsFavorite = !string.IsNullOrEmpty(userId) && grammar.FavoriteByUsers != null && 
                                 grammar.FavoriteByUsers.Contains(userId);
            
            return View(grammar);
        }
    }
}
