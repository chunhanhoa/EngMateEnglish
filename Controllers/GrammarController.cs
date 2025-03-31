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
    public class GrammarController : Controller
    {
        private readonly ILogger<GrammarController> _logger;
        private readonly HocTiengAnhContext _context;

        public GrammarController(ILogger<GrammarController> logger, HocTiengAnhContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Truy vấn tất cả dữ liệu ngữ pháp từ database
                var nguPhaps = await _context.NguPhaps
                    .Include(n => n.IdCdNavigation)
                    .ToListAsync();
                
                _logger.LogInformation($"Tìm thấy {nguPhaps.Count} bài ngữ pháp trong database.");
                
                // Chuyển đổi từ NguPhap sang GrammarModel để hiển thị trên view
                var grammarLessons = nguPhaps.Select(np => new GrammarModel
                {
                    ID_NP = np.IdNp,
                    Title_NP = np.TitleNp ?? string.Empty,
                    Description_NP = np.DiscriptionNp ?? string.Empty,
                    TimeUpload_NP = np.TimeuploadNp.HasValue ? 
                        new DateTime(np.TimeuploadNp.Value.Year, np.TimeuploadNp.Value.Month, np.TimeuploadNp.Value.Day) : null,
                    ID_CD = np.IdCd,
                    TopicName = np.IdCdNavigation?.NameCd ?? "Chưa phân loại",
                    Level = "A1", // Default level
                    Content = np.ContentNp ?? string.Empty // Sử dụng ContentNp để hiển thị nội dung HTML
                }).ToList();

                return View(grammarLessons);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi truy vấn dữ liệu ngữ pháp từ database");
                return View(new List<GrammarModel>());
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var nguPhap = await _context.NguPhaps
                    .Include(n => n.IdCdNavigation)
                    .FirstOrDefaultAsync(g => g.IdNp == id);
                
                if (nguPhap == null)
                {
                    _logger.LogWarning($"Không tìm thấy bài ngữ pháp với ID = {id}");
                    return NotFound();
                }

                // Chuyển đổi từ NguPhap sang GrammarModel
                var grammarModel = new GrammarModel
                {
                    ID_NP = nguPhap.IdNp,
                    Title_NP = nguPhap.TitleNp ?? string.Empty,
                    Description_NP = nguPhap.DiscriptionNp ?? string.Empty,
                    TimeUpload_NP = nguPhap.TimeuploadNp.HasValue ?
                        new DateTime(nguPhap.TimeuploadNp.Value.Year, nguPhap.TimeuploadNp.Value.Month, nguPhap.TimeuploadNp.Value.Day) : null,
                    ID_CD = nguPhap.IdCd,
                    TopicName = nguPhap.IdCdNavigation?.NameCd ?? "Chưa phân loại",
                    Level = "A1", // Default level
                    Content = nguPhap.ContentNp ?? string.Empty // Sử dụng ContentNp cho nội dung chi tiết
                };

                return View(grammarModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi truy vấn chi tiết ngữ pháp với ID = {id}");
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
