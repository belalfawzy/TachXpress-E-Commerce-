using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TechXpress_DepiGraduation.Data.Cart;
using TechXpress_DepiGraduation.Data.Services;
using TechXpress_DepiGraduation.Data.StaticMembers;
using TechXpress_DepiGraduation.Models;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

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
        [HttpPost]
        [Authorize(Roles = UserRoles.Admin)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, List<IFormFile> NewImages, List<string> ImagesToDelete)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            ModelState.Remove("Category");
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

                    // Fetch existing product
                    var existingProduct = await _productService.GetItemByIdAsync(id);
                    if (existingProduct == null)
                    {
                        TempData["ErrorMessage"] = "Product not found.";
                        return NotFound();
                    }

                    // Update scalar properties
                    existingProduct.Name = product.Name;
                    existingProduct.Description = product.Description;
                    existingProduct.Price = product.Price;
                    existingProduct.CategoryId = product.CategoryId;
                    existingProduct.color = product.color;

                    // Handle images
                    var updatedImages = product.Image?.Where(img => !string.IsNullOrWhiteSpace(img) && !ImagesToDelete.Contains(ExtractPublicIdFromUrl(img))).ToList() ?? new List<string>();
                    var newPublicIds = new List<string>(); // Track new public IDs for rollback

                    try
                    {
                        // Delete removed images from Cloudinary
                        foreach (var publicId in ImagesToDelete.Where(id => !string.IsNullOrWhiteSpace(id)))
                        {
                            var deletionParams = new DeletionParams(publicId);
                            var deletionResult = await _cloudinary.DestroyAsync(deletionParams);
                            if (deletionResult.StatusCode != System.Net.HttpStatusCode.OK)
                            {
                                throw new Exception($"Failed to delete image with public ID: {publicId}");
                            }
                        }

                        // Upload new images
                        foreach (var file in NewImages.Where(f => f != null && f.Length > 0))
                        {
                            var imageUrl = await _cloudinary.UploadImageAsync(file);
                            updatedImages.Add(imageUrl);
                            newPublicIds.Add(ExtractPublicIdFromUrl(imageUrl));
                        }

                        existingProduct.Image = updatedImages;
                        await _productService.EditAsync(existingProduct);
                        TempData["SuccessMessage"] = "Product updated successfully!";
                        return RedirectToAction(nameof(Index));
                    }
                    catch (Exception ex)
                    {
                        // Rollback: Delete newly uploaded images
                        foreach (var publicId in newPublicIds)
                        {
                            if (!string.IsNullOrWhiteSpace(publicId))
                            {
                                var deletionParams = new DeletionParams(publicId);
                                await _cloudinary.DestroyAsync(deletionParams);
                            }
                        }
                        throw new Exception($"Failed to update product: {ex.Message}", ex);
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