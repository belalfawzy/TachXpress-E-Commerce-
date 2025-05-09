using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace TechXpress_DepiGraduation.Data.Services
{
    public class CloudinaryIntegration
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryIntegration(IConfiguration configuration)
        {
            var account = new Account(
                configuration["Cloudinary:CloudName"],
                configuration["Cloudinary:ApiKey"],
                configuration["Cloudinary:ApiSecret"]
            );
            _cloudinary = new Cloudinary(account);
            _cloudinary.Api.Secure = true;
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            // Validate file
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("No file provided or file is empty.");
            }

            // Validate file type (only allow images)
            var validImageTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/bmp" };
            if (!validImageTypes.Contains(file.ContentType.ToLower()))
            {
                throw new ArgumentException("Invalid file type. Only JPEG, PNG, GIF, and BMP are allowed.");
            }

            // Validate file size (e.g., max 5MB)
            if (file.Length > 5 * 1024 * 1024)
            {
                throw new ArgumentException("File size exceeds 5MB limit.");
            }

            try
            {
                using var stream = file.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = "ecommerce_products",
                    PublicId = $"product_{Guid.NewGuid()}"
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                // Verify upload success
                if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new Exception($"Cloudinary upload failed: {uploadResult.Error?.Message ?? "Unknown error"}");
                }

                // Verify the URL is valid
                if (string.IsNullOrEmpty(uploadResult.SecureUrl?.ToString()))
                {
                    throw new Exception("Cloudinary returned an empty URL.");
                }

                return uploadResult.SecureUrl.ToString();
            }
            catch (Exception ex)
            {
                // Log the error (in production, use a logging framework like Serilog or NLog)
                Console.WriteLine($"Cloudinary upload error: {ex.Message}");
                throw new Exception($"Failed to upload image to Cloudinary: {ex.Message}", ex);
            }
        }

        public string GetImageUrl(string publicId, int? width = null, int? height = null)
        {
            if (string.IsNullOrEmpty(publicId))
            {
                throw new ArgumentException("PublicId cannot be empty.");
            }

            var transformation = new Transformation();
            if (width.HasValue || height.HasValue)
            {
                transformation = transformation.Width(width).Height(height).Crop("fit");
            }

            return _cloudinary.Api.UrlImgUp.Transform(transformation).BuildUrl(publicId);
        }
    }
}