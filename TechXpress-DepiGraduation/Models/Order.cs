using TechXpress_DepiGraduation.Data.Base;

namespace TechXpress_DepiGraduation.Models
{
    public class Order : IBaseEntity
    {

        public int Id { get; set; }
        public string PaymentStatus { get; set; }
        public string UserId { get; set; }
        public AppUser User { get; set; }
        public List<OrderItem>OrderItems { get; set; }
    }
}
