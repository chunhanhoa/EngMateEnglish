using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TiengAnh.Services;

namespace TiengAnh.Controllers
{
    [Authorize(Roles = "Admin")]
    public class StatisticsController : Controller
    {
        private readonly UserStatisticsService _statisticsService;
        private readonly ILogger<StatisticsController> _logger;

        public StatisticsController(
            UserStatisticsService statisticsService,
            ILogger<StatisticsController> logger)
        {
            _statisticsService = statisticsService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var statistics = await _statisticsService.GetDashboardStatisticsAsync();
                return View(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading statistics: {ex.Message}");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi tải thống kê.";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetStatisticsData()
        {
            try
            {
                var statistics = await _statisticsService.GetDashboardStatisticsAsync();
                return Json(new { success = true, data = statistics });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting statistics data: {ex.Message}");
                return Json(new { success = false, message = "Lỗi khi tải dữ liệu thống kê" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUserGrowthChart(int days = 30)
        {
            try
            {
                var chartData = await _statisticsService.GetUserGrowthChartDataAsync(days);
                return Json(new { success = true, data = chartData });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting user growth chart: {ex.Message}");
                return Json(new { success = false, message = "Lỗi khi tải biểu đồ tăng trưởng" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetActivityChart(int days = 7)
        {
            try
            {
                var chartData = await _statisticsService.GetActivityChartDataAsync(days);
                return Json(new { success = true, data = chartData });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting activity chart: {ex.Message}");
                return Json(new { success = false, message = "Lỗi khi tải biểu đồ hoạt động" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveDailyStatistics()
        {
            try
            {
                await _statisticsService.SaveDailyStatisticsAsync();
                return Json(new { success = true, message = "Lưu thống kê hàng ngày thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error saving daily statistics: {ex.Message}");
                return Json(new { success = false, message = "Lỗi khi lưu thống kê" });
            }
        }
    }
}
