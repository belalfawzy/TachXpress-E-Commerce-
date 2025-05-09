using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TechXpress_DepiGraduation.Data.Services;
using TechXpress_DepiGraduation.Data.StaticMembers;
using TechXpress_DepiGraduation.Models;
using TechXpress_DepiGraduation.Data.Services;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TechXpress_DepiGraduation.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly CloudinaryIntegration _cloudinary;

        public ProductController(IProductService productService, ICategoryService categoryService, CloudinaryIntegration cloudinaryService)
        {
            _productService = productService;
            _categoryService = categoryService;
            _cloudinary = cloudinaryService;
        }

        public async Task<IActionResult> Index(string searchQuery)
        {
            var products = await _productService.GetAllAsync();
            if (!string.IsNullOrEmpty(searchQuery))
            {
                searchQuery = searchQuery.ToLower();
                products = products.Where(p => p.Name.ToLower().Contains(searchQuery) || p.Description.ToLower().Contains(searchQuery)).ToList();
            }
            return View(products);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _categoryService.GetAllAsync();
            return View();
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> Create(Product product, IFormFile[] imageFiles)
        {
            ModelState.Remove("Category");
            ModelState.Remove("Image");

            if (ModelState.IsValid)
            {
                try
                {
                    await _productService.CreateAsync(product, imageFiles);
                    TempData["SuccessMessage"] = "Product created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Failed to create product: {ex.Message}";
                }
            }

            ViewBag.Categories = await _categoryService.GetAllAsync();
            return View(product);
        }

        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var p = await _productService.GetItemByIdAsync(id);
                if (p == null) return View("Item already not found");
                await _productService.DeleteAsync(id);
                TempData["SuccessMessage"] = "Product deleted successfully!";
            }
            catch(Exception e)
            {
                TempData["ErrorMessage"] = "Cannot delete the product";
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var p = await _productService.GetItemByIdAsync(id);
            if (p == null)
            {   TempData["ErrorMessage"] = "Product not found.";
                return NotFound();
            }

            p.color = p.color[0].Split(',').ToList();

            p.Category = await _categoryService.GetItemByIdAsync(p.CategoryId);
            return View(p);
        }
        private bool IsValidColor(string color)
        {
            // Check for hex colors
            if (color.StartsWith("#") && (color.Length == 4 || color.Length == 7))
                return Regex.IsMatch(color, @"^#[0-9A-Fa-f]{3,6}$");

            // Check for named colors (expand as needed)
            var validNamedColors = new[] { "red", "blue", "green", "yellow", "black", "white", "purple", "orange" };
            return validNamedColors.Contains(color.ToLower());

            // Add checks for rgb, rgba, etc., if neede
        }

        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> Edit(int id)
        {
            var p = await _productService.GetItemByIdAsync(id);
            if (p == null) return NotFound("you are in get method");
            return View(p);
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> Edit(Product p)
        {
            ModelState.Remove("Category");
            if (ModelState.IsValid)
            {
                await _productService.EditAsync(p);
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return NotFound(p);
            }
        }
    }
}