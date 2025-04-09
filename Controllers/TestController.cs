using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TiengAnh.Data;
using TiengAnh.Models;
using System.Security.Claims;

namespace TiengAnh.Controllers
{
    public class TestController : Controller
    {
        private readonly HocTiengAnhContext _context;

        public TestController(HocTiengAnhContext context)
        {
            _context = context;
        }

        // GET: /Test - Show list of available tests
        public async Task<IActionResult> Index()
        {
            var tests = await _context.KiemTras
                .Include(t => t.IdCdNavigation)
                .ToListAsync();
            return View(tests);
        }

        // GET: /Test/Details/5 - View test details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var test = await _context.KiemTras
                .Include(t => t.IdCdNavigation)
                .FirstOrDefaultAsync(m => m.IdKt == id);
                
            if (test == null)
            {
                return NotFound();
            }

            return View(test);
        }

        // GET: /Test/Take/5 - Take a test
        public async Task<IActionResult> Take(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var test = await _context.KiemTras
                .Include(t => t.IdCdNavigation)
                .Include(t => t.CauHoiKts)
                .FirstOrDefaultAsync(m => m.IdKt == id);
                
            if (test == null)
            {
                return NotFound();
            }

            return View(test);
        }

        // POST: /Test/SubmitTest - Submit test answers
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitTest(int testId, IFormCollection form)
        {
            // Get test info
            var test = await _context.KiemTras
                .Include(t => t.CauHoiKts)
                .Include(t => t.IdCdNavigation)
                .FirstOrDefaultAsync(m => m.IdKt == testId);
                
            if (test == null)
            {
                return NotFound();
            }

            // Get current user ID from claims
            int userId = 0;
            var userIdStr = User.FindFirstValue("UserID");
            if (int.TryParse(userIdStr, out int parsedUserId))
            {
                userId = parsedUserId;
            }
            else
            {
                // Fallback to default if not logged in
                userId = 4; // Default user ID
            }

            // Create a dictionary to store answers
            Dictionary<int, string> answers = new Dictionary<int, string>();
            
            // Extract answers from form
            foreach (var key in form.Keys)
            {
                if (key.StartsWith("answers_"))
                {
                    string idStr = key.Substring("answers_".Length);
                    if (int.TryParse(idStr, out int questionId))
                    {
                        answers[questionId] = form[key];
                    }
                }
            }

            // Calculate score
            int totalQuestions = test.CauHoiKts.Count;
            int correctAnswers = 0;
            
            // Create test result
            var testResult = new KetQuaKiemTra
            {
                IdKt = testId,
                IdTk = userId,
                FinishTimeKq = DateTime.Now
            };
            
            _context.KetQuaKiemTras.Add(testResult);
            await _context.SaveChangesAsync();
            
            // Process each question
            foreach (var question in test.CauHoiKts)
            {
                string userAnswer = answers.ContainsKey(question.IdCh) ? answers[question.IdCh] : null;
                bool isCorrect = userAnswer == question.AnswerCh;
                
                if (isCorrect)
                {
                    correctAnswers++;
                }
                
                // Save answer detail
                var answerDetail = new ChiTietKetQua
                {
                    IdKq = testResult.IdKq,
                    IdCh = question.IdCh,
                    UserAnswerCtkq = userAnswer,
                    IsCorrectCtkq = isCorrect
                };
                
                _context.ChiTietKetQuas.Add(answerDetail);
            }
            
            // Update score
            int score = (int)Math.Round((double)correctAnswers / totalQuestions * 100);
            testResult.ScoreKq = score;
            _context.Update(testResult);
            
            // Lưu tiến trình học tập
            await SaveLearningProgress(userId, testId, test.IdCd, "KiemTra", score);
            
            await _context.SaveChangesAsync();
            
            // Redirect to results
            return RedirectToAction("Results", new { id = testResult.IdKq });
        }
        
        // Phương thức lưu tiến trình học tập
        private async Task SaveLearningProgress(int userId, int contentId, int topicId, string type, int score)
        {
            // Kiểm tra xem tiến trình đã tồn tại chưa
            var existingProgress = await _context.TienTrinhHocs
                .FirstOrDefaultAsync(t => t.IdTk == userId && t.TypeTth == type && t.IdTypeTth == contentId);

            if (existingProgress != null)
            {
                // Cập nhật tiến trình hiện tại
                existingProgress.LastTimeStudyTth = DateTime.Now;
                existingProgress.ScoreTth = score;
                _context.TienTrinhHocs.Update(existingProgress);
            }
            else
            {
                // Tạo tiến trình mới
                var newProgress = new TienTrinhHoc
                {
                    IdTk = userId,
                    IdTypeTth = contentId,
                    TypeTth = type,
                    LastTimeStudyTth = DateTime.Now,
                    ScoreTth = score,
                    IdCd = topicId
                };
                _context.TienTrinhHocs.Add(newProgress);
            }
        }

        // GET: /Test/Results/5 - Show test results
        public async Task<IActionResult> Results(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var result = await _context.KetQuaKiemTras
                .Include(r => r.IdKtNavigation)
                .Include(r => r.IdTkNavigation)
                .Include(r => r.ChiTietKetQuas)
                    .ThenInclude(d => d.IdChNavigation)
                .FirstOrDefaultAsync(m => m.IdKq == id);
                
            if (result == null)
            {
                return NotFound();
            }

            return View(result);
        }
    }
}
