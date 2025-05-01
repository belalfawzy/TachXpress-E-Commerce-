using Microsoft.AspNetCore.Mvc;

namespace TechXpress_DepiGraduation.Controllers
{
    public class OrderController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
