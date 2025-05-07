using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Linq;
using System;
using TiengAnh.Models;
using TiengAnh.Repositories;
using MongoDB.Bson;

namespace TiengAnh.Controllers
{
    [Authorize(Roles = "Admin")]
    public class GrammarAdminController : Controller
    {
        private readonly GrammarRepository _grammarRepository;

        public GrammarAdminController(GrammarRepository grammarRepository)
        {
            _grammarRepository = grammarRepository;
        }

        public async Task<IActionResult> Index()
        {
            var grammars = await _grammarRepository.GetAllAsync();
            return View(grammars);
        }

        public IActionResult Create()
        {
            var grammar = new GrammarModel
            {
                Level = "A1",
                Created = DateTime.Now,
                TimeUpload_NP = DateTime.Now
            };
            return View(grammar);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GrammarModel grammar)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Generate a new ID (highest ID + 1)
                    var allGrammars = await _grammarRepository.GetAllAsync();
                    grammar.ID_NP = allGrammars.Count > 0 ? allGrammars.Max(g => g.ID_NP) + 1 : 1;
                    
                    // Ensure created date is set
                    if (grammar.Created == null)
                    {
                        grammar.Created = DateTime.Now;
                    }
                    
                    // Ensure TimeUpload_NP is set
                    grammar.TimeUpload_NP = DateTime.Now;
                    
                    // Set default values for required fields
                    if (string.IsNullOrEmpty(grammar.TopicName))
                    {
                        grammar.TopicName = "Grammar";
                    }
                    
                    // Initialize collections
                    grammar.FavoriteByUsers = new List<string>();
                    
                    await _grammarRepository.CreateAsync(grammar);
                    
                    TempData["SuccessMessage"] = $"Ngữ pháp '{grammar.Title_NP}' đã được thêm thành công.";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Lỗi khi thêm bài ngữ pháp: {ex.Message}");
                }
            }
            
            return View(grammar);
        }

        public async Task<IActionResult> Edit(int id)
        {
            // Use GetByGrammarIdAsync to find by ID_NP
            var grammar = await _grammarRepository.GetByGrammarIdAsync(id);
            if (grammar == null)
            {
                return NotFound();
            }
            
            return View(grammar);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(GrammarModel grammar)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Get the existing grammar to preserve its MongoDB Id and other properties
                    var existingGrammar = await _grammarRepository.GetByGrammarIdAsync(grammar.ID_NP);
                    if (existingGrammar == null)
                    {
                        return NotFound();
                    }
                    
                    // Preserve MongoDB Id
                    grammar.Id = existingGrammar.Id;
                    
                    // Preserve other properties if they're not set in the form
                    if (grammar.FavoriteByUsers == null)
                        grammar.FavoriteByUsers = existingGrammar.FavoriteByUsers;
                    
                    if (string.IsNullOrEmpty(grammar.TopicName))
                        grammar.TopicName = existingGrammar.TopicName;
                    
                    if (grammar.Created == null)
                        grammar.Created = existingGrammar.Created;
                        
                    if (grammar.TimeUpload_NP == default)
                        grammar.TimeUpload_NP = existingGrammar.TimeUpload_NP;
                    
                    // Update using the MongoDB document Id
                    await _grammarRepository.UpdateAsync(grammar.Id, grammar);
                    
                    TempData["SuccessMessage"] = $"Ngữ pháp '{grammar.Title_NP}' đã được cập nhật thành công.";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Lỗi khi cập nhật bài ngữ pháp: {ex.Message}");
                }
            }
            
            return View(grammar);
        }

        public async Task<IActionResult> Delete(int id)
        {
            // Use GetByGrammarIdAsync instead to find by ID_NP
            var grammar = await _grammarRepository.GetByGrammarIdAsync(id);
            if (grammar == null)
            {
                return NotFound();
            }
            
            return View(grammar);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Use GetByGrammarIdAsync instead to find by ID_NP
            var grammar = await _grammarRepository.GetByGrammarIdAsync(id);
            if (grammar == null)
            {
                return NotFound();
            }
            
            await _grammarRepository.DeleteAsync(grammar.Id);
            
            TempData["SuccessMessage"] = $"Ngữ pháp '{grammar.Title_NP}' đã được xóa thành công.";
            return RedirectToAction("Index");
        }
    }
}
