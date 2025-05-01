using System.ComponentModel.DataAnnotations.Schema;
using TechXpress_DepiGraduation.Data.Base;

namespace TechXpress_DepiGraduation.Models
{
    
    public class ShoppingCartItem : IBaseEntity
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public int Quantity { get; set; }
        public AppUser User { get; set; }
        public string ShoppingCartId { get; set; }
    }
}
