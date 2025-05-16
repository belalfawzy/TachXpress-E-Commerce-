using System.ComponentModel.DataAnnotations.Schema;
using TechXpress_DepiGraduation.Data.Base;

namespace TechXpress_DepiGraduation.Models
{
    public class OrderItem : IBaseEntity
    {
        public int Id { get; set; }
        public int Amount { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        public int OrderId { get; set; }
        public int productId { get; set; }
        public Product Product { get; set; }
        public Order Order { get; set; } 
    }
}
