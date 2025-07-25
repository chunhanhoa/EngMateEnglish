using Microsoft.AspNetCore.Mvc;

namespace TiengAnh.Controllers
{
    public class AIChatController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Trò chuyện AI";
            return View();
        }
    }
}