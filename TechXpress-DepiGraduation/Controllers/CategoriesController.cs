using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TechXpress_DepiGraduation.Data.Services;
using TechXpress_DepiGraduation.Data.StaticMembers;
using TechXpress_DepiGraduation.Models;

namespace TechXpress_DepiGraduation.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ICategoryService _categoryservice;
        public CategoriesController(ICategoryService service)
        {
            _categoryservice = service;
                
        }
        public async Task<IActionResult> Index()
        {
            var categories = await _categoryservice.GetAllAsync();
            return View(categories);
        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [Authorize(Roles = UserRoles.Admin)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description")]Category category)
        {
            ModelState.Remove("Products");

            if (ModelState.IsValid)
            {
                await _categoryservice.CreateAsync(category);
                return RedirectToAction(nameof(Index));

            }
            return View(category);

        }
        public async Task<IActionResult> Details(int id)
        {
            var category = await _categoryservice.GetItemByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _categoryservice.GetItemByIdAsync(id);
            if (category == null) return NotFound();
            return View(category);
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.Admin)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("Id,Name,Description")] Category category)
        {
           
            ModelState.Remove("Products");

            if (ModelState.IsValid)
            {
                await _categoryservice.EditAsync(category);
                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }
        [HttpPost]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            await _categoryservice.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }



    }
}
