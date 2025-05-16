using TechXpress_DepiGraduation.Data.Base;
using TechXpress_DepiGraduation.Data.Enums;
using TechXpress_DepiGraduation.Models;

namespace TechXpress_DepiGraduation.Data.Services
{
    public interface IOrderService 
    {
        Task StoreOrdersAsync(List<ShoppingCartItem> items, string userId,
                         int addressId, PaymentMethod paymentMethod, decimal shippingCost);
        Task<List<Order>> GetOrderAndRoleByUserIdAsync(string userId, string role);
        Task<List<Order>> GetallOrders(string userId);
        Task<Dictionary<int, List<OrderItem>>> GetorderItems(string userId);
        Task<Dictionary<int, List<OrderItem>>> Getall();
        Task UpdateOrderStatus(int orderId, OrderStatus status);
        Task<Order> GetOrderWithUserDetails(int orderId);
    }
}
