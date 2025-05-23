﻿using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using TechXpress_DepiGraduation.Data.Base;

namespace TechXpress_DepiGraduation.Models
{
    public class Product : IBaseEntity
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "this Field is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Name Must be have at least 2 chars")]
        public string Name { get; set; }
        [Required(ErrorMessage = "this Field is required")] 
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Description Must be have at least 10 chars")]
        public string Description { get; set; }
        [Required(ErrorMessage = "this Field is required")]
        [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "Price must be a valid number")]
        [Range(1, 1000000, ErrorMessage = "Price must be between 1 and 1,000,000")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Price { get; set; }
        [Required(ErrorMessage = "this Field is required")]
        public List<string> Image { get; set; }
        [Required(ErrorMessage = "Add at least one Image")]
        public List<string> color { get; set; }
        [Required(ErrorMessage =("this Field is required"))]
        public int CategoryId { get; set; }
        public Category Category { get; set; }
    }
}
