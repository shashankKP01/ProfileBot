using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
namespace ProfileBot.Controllers
{
    public class ChatbotController : Controller
    {
        public IActionResult Index(string token)
        {
            if (token == null)
                return BadRequest("Invalid token");
            ViewBag.Token = token;
            return View();
        }
    }
}