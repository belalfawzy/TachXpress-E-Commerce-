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

        [HttpGet]
        [Authorize(Roles = UserRoles.Admin)]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.Admin)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormFile ImageFile)
        {
            var newCategory = new Category
            {
                Name = Request.Form["Name"],
                Description = Request.Form["Description"]
            };

            // التحقق اليدوي من الحقول
            if (string.IsNullOrEmpty(newCategory.Name) || newCategory.Name.Length < 2)
            {
                TempData["ErrorMessage"] = "Name is required and must be at least 2 characters.";
                return View(newCategory);
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(newCategory.Name, @"^[a-zA-Z\s]+$"))
            {
                TempData["ErrorMessage"] = "Category Name must contain only letters.";
                return View(newCategory);
            }

            if (string.IsNullOrEmpty(newCategory.Description) || newCategory.Description.Length < 10)
            {
                TempData["ErrorMessage"] = "Description is required and must be at least 10 characters.";
                return View(newCategory);
            }

            // التحقق من الصورة ورفعها
            if (ImageFile != null && ImageFile.Length > 0)
            {
                var uploads = Path.Combine("wwwroot/images/categories");
                var uniqueName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                var path = Path.Combine(uploads, uniqueName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                newCategory.ImageName = uniqueName;
            }

            // التحقق من وجود الصورة
            if (string.IsNullOrEmpty(newCategory.ImageName))
            {
                TempData["ErrorMessage"] = "Please upload an image.";
                return View(newCategory);
            }


            await _categoryservice.CreateAsync(newCategory);
            //TempData["SuccessMessage"] = "Category created successfully!";
            return RedirectToAction(nameof(Index));
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
        public async Task<IActionResult> Edit(int id, IFormFile ImageFile)
        {
            var existingCategory = await _categoryservice.GetItemByIdAsync(id);
            if (existingCategory == null)
                return NotFound();

    
            existingCategory.Name = Request.Form["Name"];
            existingCategory.Description = Request.Form["Description"];

            if (string.IsNullOrEmpty(existingCategory.Name) || existingCategory.Name.Length < 2)
            {
                TempData["ErrorMessage"] = "Name is required and must be at least 2 characters.";
                return View(existingCategory);
            }

            if (string.IsNullOrEmpty(existingCategory.Description) || existingCategory.Description.Length < 10)
            {
                TempData["ErrorMessage"] = "Description is required and must be at least 10 characters.";
                return View(existingCategory);
            }

       
            if (ImageFile != null && ImageFile.Length > 0)
            {
                if (!string.IsNullOrEmpty(existingCategory.ImageName))
                {
                    var oldPath = Path.Combine("wwwroot/images/categories", existingCategory.ImageName);
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }

                var uploads = Path.Combine("wwwroot/images/categories");
                var uniqueName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                var path = Path.Combine(uploads, uniqueName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                existingCategory.ImageName = uniqueName;
            }

    
            if (string.IsNullOrEmpty(existingCategory.ImageName))
            {
                TempData["ErrorMessage"] = "Please upload an image.";
                return View(existingCategory);
            }

            await _categoryservice.EditAsync(existingCategory);
            return RedirectToAction(nameof(Index));
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
