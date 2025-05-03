using System.ComponentModel.DataAnnotations;
using TechXpress_DepiGraduation.Data.Base;

namespace TechXpress_DepiGraduation.Models
{
    public class Category : IBaseEntity
    {
        public int Id { get; set ; }
        [Required(ErrorMessage = "this Field is required")]
        [StringLength(15, MinimumLength = 2, ErrorMessage = "Name Must be have at least 2 chars")]
        public string Name { get; set; }
        [Required(ErrorMessage = "this Field is required")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Description Must be have at least 10 chars")]
        public string Description { get; set; }
        public List<Product> Products { get; set; }
        
    }
}
