using Microsoft.AspNetCore.Mvc;
using TechXpress_DepiGraduation.Data.Services;
using TechXpress_DepiGraduation.Data.ViewModel;
using TechXpress_DepiGraduation.Models;
using System.Linq;

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
            var products = await _service.GetAllAsync() ?? Enumerable.Empty<Product>();

            // Fetch categories
            var categories = await _categoryService.GetAllAsync() ?? Enumerable.Empty<Category>();
            // Create ViewModel
            var viewModel = new HomeViewModel
            {
                Products = products,
                Categories = categories
            };

            return View(viewModel);
        }
    }
}