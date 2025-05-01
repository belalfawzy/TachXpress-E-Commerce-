using Microsoft.EntityFrameworkCore;
using TechXpress_DepiGraduation.Models;

namespace TechXpress_DepiGraduation.Data.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;
        public OrderService(AppDbContext context)
        {
            _context = context;
        }
        public Task<List<Order>> GetOrderAndRoleByUserIdAsync(string userId, string role)
        {
            var orders = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(u => u.UserId == userId)
                .ToListAsync();
            return orders;
        }

        public async Task StoreOrdersAsync(List<ShoppingCartItem> items, string userId)
        {
            var Order = new Order(){ UserId = userId };
            await _context.Orders.AddAsync(Order);
            foreach(var v in items)
            {
                var orderItem = new OrderItem()
                {
                    productId = v.ProductId,
                    OrderId = Order.Id,
                    Amount = v.Quantity,
                    Price = v.Product.Price,

                };
                await _context.OrderItems.AddAsync(orderItem);
            }
            await _context.SaveChangesAsync();
        }
    }
}
