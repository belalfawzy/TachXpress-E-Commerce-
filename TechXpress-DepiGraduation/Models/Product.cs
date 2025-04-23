using TechXpress_DepiGraduation.Data.Base;

namespace TechXpress_DepiGraduation.Models
{
    public class Product : IBaseEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Image { get; set; }
        public List<string> color { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }
    }
}
