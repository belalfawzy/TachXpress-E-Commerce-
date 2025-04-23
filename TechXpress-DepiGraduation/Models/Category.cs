using TechXpress_DepiGraduation.Data.Base;

namespace TechXpress_DepiGraduation.Models
{
    public class Category : IBaseEntity
    {
        public int Id { get; set ; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<Product> Products { get; set; }
        
    }
}
