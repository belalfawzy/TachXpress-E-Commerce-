using System.ComponentModel.DataAnnotations;
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
        public decimal Price { get; set; }
        [Required(ErrorMessage = "this Field is required")]
        public List<string> Image { get; set; }
        public List<string> color { get; set; }
        [Required(ErrorMessage =("this Field is required"))]
        public int CategoryId { get; set; }
        public Category Category { get; set; }
    }
}
