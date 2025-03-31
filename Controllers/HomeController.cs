using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TiengAnh.Data;
using TiengAnh.Models;

namespace TiengAnh.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HocTiengAnhContext _context;

        public HomeController(ILogger<HomeController> logger, HocTiengAnhContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Lấy danh sách chủ đề từ database
            var chuDes = await _context.ChuDes.ToListAsync();
            
            // Truyền dữ liệu chủ đề vào ViewBag để sử dụng trong giao diện phong phú
            ViewBag.Topics = chuDes;
            
            // Vẫn truyền Model như cũ để tương thích với code hiện tại
            return View(chuDes);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
