using TechXpress_DepiGraduation.Data.Base;
using TechXpress_DepiGraduation.Data.Enums;

namespace TechXpress_DepiGraduation.Models
{
    public class Order : IBaseEntity
    {

        public int Id { get; set; }
        public OrderStatus OrderStatus { get; set; } = OrderStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string UserId { get; set; }
        public AppUser User { get; set; }
        public List<OrderItem>OrderItems { get; set; }
    }
}
