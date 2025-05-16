using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using TechXpress_DepiGraduation.Data.Base;
using TechXpress_DepiGraduation.Data.Enums;

namespace TechXpress_DepiGraduation.Models
{
    public class Order : IBaseEntity
    {

        public int Id { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string UserId { get; set; }
        public AppUser User { get; set; }
        public int AddressesId { get; set; }
        public Addresses ShippingAddress { get; set; }
     //   public string? PaypalEmail { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal ShippingCost { get; set; }

        public List<OrderItem>OrderItems { get; set; }
    }
}
