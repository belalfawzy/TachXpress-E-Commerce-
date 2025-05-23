﻿using System.ComponentModel.DataAnnotations;
using TechXpress_DepiGraduation.Data.Base;

namespace TechXpress_DepiGraduation.Models
{
    public class Category : IBaseEntity
    {
        public int Id { get; set ; }
        [Required(ErrorMessage = "this Field is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Name Must be have at least 2 chars")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Category Name must contain only letters")]
        public string Name { get; set; }
        [Required(ErrorMessage = "this Field is required")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Description Must be have at least 10 chars")]
        public string Description { get; set; }
        public List<Product> Products { get; set; }

        public string ImageName { get; set; } 

    }
}
