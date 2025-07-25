using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TiengAnh.Controllers
{
    [Authorize]
    public class AIChatController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Trò chuyện AI";
            return View();
        }
    }
}
