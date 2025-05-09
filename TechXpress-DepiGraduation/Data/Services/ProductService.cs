using Microsoft.EntityFrameworkCore;
using TechXpress_DepiGraduation.Data.Base;
using TechXpress_DepiGraduation.Models;

namespace TechXpress_DepiGraduation.Data.Services
{
    public class ProductService : BaseRepositoryEntity<Product>, IProductService
    {
        private readonly CloudinaryIntegration _cloudinary;
        public ProductService(AppDbContext context, CloudinaryIntegration cloudinary) : base(context)
        {
            _cloudinary = cloudinary;
        }
        
        public new async Task CreateAsync(Product product,IFormFile[] imageFiles)
         {
            // Validate product
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product), "Product cannot be null.");
            }

            if (imageFiles == null || !imageFiles.Any())
            {
                throw new ArgumentException("At least one image is required for the product.");
            }

            // Upload images to Cloudinary
            product.Image = new List<string>();
            foreach (var file in imageFiles)
            {
                if (file != null && file.Length > 0)
                {
                    try
                    {
                        var imageUrl = await _cloudinary.UploadImageAsync(file);
                        product.Image.Add(imageUrl);
                    }
                    catch (Exception ex)
                    {
                        // Log error and rethrow with context
                        Console.WriteLine($"Failed to upload image: {ex.Message}");
                        throw new Exception($"Error uploading image: {ex.Message}", ex);
                    }
                }
            }

            if (!product.Image.Any())
            {
                throw new ArgumentException("No images were uploaded successfully.");
            }

            // Validate CategoryId
            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == product.CategoryId);
            if (!categoryExists)
            {
                throw new ArgumentException("Invalid CategoryId provided.");
            }

            // Save product to database
            try
            {
                await base.CreateAsync(product);
            }
            catch (Exception ex)
            {
                // Log database error
                Console.WriteLine($"Database save error: {ex.Message}");
                throw new Exception($"Failed to save product to database: {ex.Message}", ex);
            }
        }

    }
}
