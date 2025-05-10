using TechXpress_DepiGraduation.Models;

namespace TechXpress_DepiGraduation.Data.ViewModel
{
    public class OrderListViewModel
    {
        public bool IsAdmin { get; set; }
        public string UserId { get; set; }
        public List<Order> Orders { get; set; }
    }
}
