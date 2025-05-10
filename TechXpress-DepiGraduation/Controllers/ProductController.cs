using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices.JavaScript;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TechXpress_DepiGraduation.Data.Cart;
using TechXpress_DepiGraduation.Data.Services;
using TechXpress_DepiGraduation.Data.StaticMembers;
using TechXpress_DepiGraduation.Models;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace TechXpress_DepiGraduation.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly ShoppingCart _shoppingCart;
        private readonly CloudinaryIntegration _cloudinary;

        public ProductController(IProductService productService, ICategoryService categoryService, ShoppingCart shoppingCart, CloudinaryIntegration cloudinaryService)
        {
            _productService = productService;
            _categoryService = categoryService;
            _shoppingCart = shoppingCart;
            _cloudinary = cloudinaryService;
        }

        // GET: /Product/
        public async Task<IActionResult> Index(string searchQuery)
        {
            var products = await _productService.GetAllAsync();
            if (!string.IsNullOrEmpty(searchQuery))
            {
                searchQuery = searchQuery.ToLower();
                products = products.Where(p => p.Name.ToLower().Contains(searchQuery) || p.Description.ToLower().Contains(searchQuery)).ToList();
            }

            var cartItems = _shoppingCart.GetShoppingCartItems();
            var quantities = cartItems.ToDictionary(i => i.Product.Id, i => i.Quantity);
            ViewBag.CartQuantities = quantities;
            return View(products);
        }

        // GET: /Product/Create
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _categoryService.GetAllAsync();
            return View();
        }

        // POST: /Product/Create
        [HttpPost]
        [Authorize(Roles = UserRoles.Admin)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile[] imageFiles)
        {
            ModelState.Remove("Category");
            ModelState.Remove("Image");

            if (ModelState.IsValid)
            {
                try
                {
                    // Validate colors
                    var colors = product.color[0].Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
                    if (!colors.All(IsValidColor))
                    {
                        ModelState.AddModelError("color", "One or more colors are invalid.");
                        ViewBag.Categories = await _categoryService.GetAllAsync();
                        return View(product);
                    }

                    var imageUrls = new List<string>();
                    var publicIds = new List<string>(); // Track public IDs for rollback
                    try
                    {
                        // Upload images
                        foreach (var file in imageFiles.Where(f => f != null && f.Length > 0))
                        {
                            var imageUrl = await _cloudinary.UploadImageAsync(file);
                            imageUrls.Add(imageUrl);
                            publicIds.Add(ExtractPublicIdFromUrl(imageUrl));
                        }

                        product.Image = imageUrls;
                        await _productService.CreateAsync(product);
                        TempData["SuccessMessage"] = "Product created successfully!";
                        return RedirectToAction(nameof(Index));
                    }
                    catch (Exception ex)
                    {
                        // Rollback: Delete uploaded images
                        foreach (var publicId in publicIds)
                        {
                            if (!string.IsNullOrWhiteSpace(publicId))
                            {
                                var deletionParams = new DeletionParams(publicId);
                                await _cloudinary.DestroyAsync(deletionParams);
                            }
                        }
                        throw new Exception($"Failed to create product: {ex.Message}", ex);
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = ex.Message;
                }
            }

            ViewBag.Categories = await _categoryService.GetAllAsync();
            return View(product);
        }

        // GET: /Product/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var product = await _productService.GetItemByIdAsync(id);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Product not found.";
                return NotFound();
            }

            ViewBag.SimilarProducts = _productService.getSimilar(4, product.CategoryId);

            product.color = product.color[0].Split(',').ToList();

            product.Category = await _categoryService.GetItemByIdAsync(product.CategoryId);
            return View(product);
        }

        // GET: /Product/Edit/5
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productService.GetItemByIdAsync(id);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Product not found.";
                return NotFound();
            }
            ViewBag.Categories = await _categoryService.GetAllAsync();
            return View(product);
        }

        // POST: /Product/Edit
        
// POST: /Product/Edit
[HttpPost]
[Authorize(Roles = UserRoles.Admin)]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Edit(int id, Product product, IFormFile[] imageFiles, string color, string[] ExistingImages, string ImagesToDelete)
{
    if (id != product.Id)
    {
        TempData["ErrorMessage"] = "Invalid product ID.";
        return NotFound();
    }

    // Remove Category and Image from ModelState validation
    ModelState.Remove("Category");
    ModelState.Remove("Image");
    ModelState.Remove("ImagesToDelete");

    // Parse and validate colors
    List<string> colors = string.IsNullOrWhiteSpace(color)
        ? new List<string>()
        : color.Split(',', StringSplitOptions.RemoveEmptyEntries)
              .Select(c => c.Trim())
              .Where(c => !string.IsNullOrWhiteSpace(c))
              .ToList();

    if (colors.Any() && !colors.All(IsValidColor))
    {
        ModelState.AddModelError("color", "One or more colors are invalid.");
    }

    if (!colors.Any())
    {
        ModelState.AddModelError("color", "At least one color is required.");
    }

    // Retrieve the existing product from the database
    if (ImagesToDelete == null) ImagesToDelete = "";
    if (ModelState.IsValid)
    {
        try
        {

            // Handle images
            // Initialize with existing images that are still present
            var updatedImages = ExistingImages?.Where(img => !string.IsNullOrWhiteSpace(img)).ToList() ?? new List<string>();
            Console.WriteLine($"Existing images from form: {string.Join(", ", updatedImages)}"); // Debug

            // Parse images to delete
            var imagesToDelete = string.IsNullOrWhiteSpace(ImagesToDelete)
                ? new List<string>()
                : ImagesToDelete.Split(',', StringSplitOptions.RemoveEmptyEntries)
                               .Select(img => img.Trim())
                               .Where(img => !string.IsNullOrWhiteSpace(img))
                               .ToList();

            // Validate at least one image will remain
            var newImageCount = imageFiles?.Count(f => f != null && f.Length > 0)??0;
            if (updatedImages.Count + newImageCount < 1)
            {
                ModelState.AddModelError("Image", "At least one image is required.");
                ViewBag.Categories = await _categoryService.GetAllAsync();
                return View(product);
            }

            var newPublicIds = new List<string>(); // Track new uploads for rollback

            try
            {
                foreach (var file in imageFiles.Where(f => f != null && f.Length > 0))
                {
                    var imageUrl = await _cloudinary.UploadImageAsync(file);
                    updatedImages.Add(imageUrl);
                    var publicId = ExtractPublicIdFromUrl(imageUrl);
                    newPublicIds.Add(publicId);
                    Console.WriteLine($"Uploaded image: {imageUrl}"); 
                }

                
                foreach (var imageUrl in imagesToDelete)
                {
                    var publicId = ExtractPublicIdFromUrl(imageUrl);
                    if (!string.IsNullOrWhiteSpace(publicId))
                    {
                        Console.WriteLine($"Deleting image with public ID: {publicId}"); // Debug
                        var deletionParams = new DeletionParams(publicId);
                        var deletionResult = await _cloudinary.DestroyAsync(deletionParams);
                        if (deletionResult.StatusCode != System.Net.HttpStatusCode.OK)
                        {
                            Console.WriteLine($"Failed to delete image: {publicId}"); // Debug
                        }
                    }
                }

                product.Image = updatedImages;
                Console.WriteLine($"Final updated images: {string.Join(", ", product.Image)}"); // Debug

                await _productService.EditAsync(product);
                TempData["SuccessMessage"] = "Product updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Rollback: Delete newly uploaded images
                foreach (var publicId in newPublicIds.Where(id => !string.IsNullOrWhiteSpace(id)))
                {
                    Console.WriteLine($"Rolling back image with public ID: {publicId}"); // Debug
                    var deletionParams = new DeletionParams(publicId);
                    await _cloudinary.DestroyAsync(deletionParams);
                }
                throw new Exception($"Failed to update product: {ex.Message}", ex);
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error updating product: {ex.Message}";
            Console.WriteLine($"Error: {ex.Message}"); // Debug
        }
    }

    ViewBag.Categories = await _categoryService.GetAllAsync();
    product.color = colors; 
    product.Image = ExistingImages?.ToList() ?? product.Image; // Preserve images for view
    return View(product);
}






        // GET: /Product/Delete/5
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var product = await _productService.GetItemByIdAsync(id);
                if (product == null)
                {
                    TempData["ErrorMessage"] = "Product not found.";
                    return NotFound("Item already not found");
                }

                // Delete images from Cloudinary
                if (product.Image != null)
                {
                    foreach (var imageUrl in product.Image)
                    {
                        var publicId = ExtractPublicIdFromUrl(imageUrl);
                        if (!string.IsNullOrWhiteSpace(publicId))
                        {
                            var deletionParams = new DeletionParams(publicId);
                            var deletionResult = await _cloudinary.DestroyAsync(deletionParams);
                            if (deletionResult.StatusCode != System.Net.HttpStatusCode.OK)
                            {
                                throw new Exception($"Failed to delete image with public ID: {publicId}");
                            }
                        }
                    }
                }

                await _productService.DeleteAsync(id);
                TempData["SuccessMessage"] = "Product deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Cannot delete the product: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool IsValidColor(string color)
        {
            if (string.IsNullOrWhiteSpace(color))
                return false;

            // Check for hex colors
            if (color.StartsWith("#") && (color.Length == 4 || color.Length == 7))
                return Regex.IsMatch(color, @"^#[0-9A-Fa-f]{3,6}$");

            // Check for named colors
            var validNamedColors = new[] { "red", "blue", "green", "yellow", "black", "white", "purple", "orange" };
            return validNamedColors.Contains(color.ToLower());
        }

        private string ExtractPublicIdFromUrl(string imageUrl, string folder = "ecommerce_products")
        {
            if (string.IsNullOrEmpty(imageUrl))
                return string.Empty;

            try
            {
                var uri = new Uri(imageUrl);
                var path = uri.AbsolutePath; // e.g., /image/upload/v1234567890/ecommerce_products/product_guid.jpg
                var segments = path.Split('/').Where(s => !string.IsNullOrEmpty(s)).ToList();
                var index = segments.IndexOf("upload") + 1; // Move past 'upload' and version
                if (segments.Count > index + 1 && segments[index] == folder)
                {
                    return $"{folder}/{segments.Last().Split('.')[0]}"; // e.g., ecommerce_products/product_guid
                }
                return segments.Last().Split('.')[0]; // Fallback
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}