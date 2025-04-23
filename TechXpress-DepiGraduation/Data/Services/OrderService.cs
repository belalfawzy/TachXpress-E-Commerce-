using TechXpress_DepiGraduation.Models;

namespace TechXpress_DepiGraduation.Data.Services
{
    public class OrderService : IOrderService
    {
        public Task<List<Order>> GetOrderAndRoleByUserIdAsync(string userId, string role)
        {
            throw new NotImplementedException();
        }

        public Task StoreOrdersAsync(List<ShoppingCartItem> items, string userId)
        {
            throw new NotImplementedException();
        }
    }
}
