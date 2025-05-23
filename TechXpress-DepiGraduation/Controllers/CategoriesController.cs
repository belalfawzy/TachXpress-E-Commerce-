﻿using Microsoft.AspNetCore.Authorization;
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
        public async Task<IActionResult> Create(Category category, IFormFile ImageFile)
        {
            
            ModelState.Remove("ImageName");

           
            if (ImageFile == null || ImageFile.Length == 0)
            {
                ModelState.AddModelError("ImageName", "Please upload an image.");
            }
            else
            {
                try
                {
                    var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/categories");
                    if (!Directory.Exists(uploads))
                    {
                        Directory.CreateDirectory(uploads);
                    }
                    var uniqueName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                    var path = Path.Combine(uploads, uniqueName);

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }

                    category.ImageName = uniqueName;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("ImageName", $"Failed to upload image: {ex.Message}");
                }
            }

          
            ModelState.Remove("Products");

            if (!ModelState.IsValid)
            {
                return View(category);
            }

            await _categoryservice.CreateAsync(category);
            TempData["SuccessMessage"] = "Category created successfully!";
            return RedirectToAction(nameof(Index));
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

            // Validation
            if (string.IsNullOrEmpty(existingCategory.Name) || existingCategory.Name.Length < 2)
            {
                ModelState.AddModelError("Name", "Name is required and must be at least 2 characters.");
                return View(existingCategory);
            }

            if (string.IsNullOrEmpty(existingCategory.Description) || existingCategory.Description.Length < 10)
            {
                ModelState.AddModelError("Description", "Description is required and must be at least 10 characters.");
                return View(existingCategory);
            }

            // Handle Image Upload
            if (ImageFile != null && ImageFile.Length > 0)
            {
                if (!string.IsNullOrEmpty(existingCategory.ImageName))
                {
                    var oldPath = Path.Combine("wwwroot/images/categories", existingCategory.ImageName);
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }

                var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/categories");
                if (!Directory.Exists(uploads))
                {
                    Directory.CreateDirectory(uploads);
                }

                var uniqueName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                var path = Path.Combine(uploads, uniqueName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                existingCategory.ImageName = uniqueName;
            }
            else if (string.IsNullOrEmpty(existingCategory.ImageName))
            {
                ModelState.AddModelError("ImageName", "Please upload an image since there is no existing image.");
                return View(existingCategory);
            }

           
            await _categoryservice.EditAsync(existingCategory);
            TempData["SuccessMessage"] = "Category updated successfully!";
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Details(int id)
        {
            var category = await _categoryservice.GetItemByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            if (category.Products == null)
            {
                category.Products = new List<Product>(); 
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
