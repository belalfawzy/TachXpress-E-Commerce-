using Microsoft.AspNetCore.Mvc;
using TechXpress_DepiGraduation.Data.Services;

namespace TechXpress_DepiGraduation.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductService _service;
        private readonly ICategoryService _categoryService;
        public HomeController(IProductService service, ICategoryService categoryService)
        {
            _service = service;
            _categoryService = categoryService;
        }
        public async Task<IActionResult> Index()
        {
            var products = await _service.GetAllAsync();

            return View(products);
        }
    }
}
