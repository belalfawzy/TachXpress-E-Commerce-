using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using TechXpress_DepiGraduation.Data.Enums;
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

        public async Task StoreOrdersAsync(List<ShoppingCartItem> items, string userId,
                                 int addressId, PaymentMethod paymentMethod, decimal shippingCost)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (items == null || !items.Any())
                    throw new ArgumentException("The Cart is Empty");

                // التحقق من وجود العنوان
                var address = await _context.Addresses.FindAsync(addressId);
                if (address == null || address.UserId != userId)
                    throw new InvalidOperationException("Invalid Address");

                foreach (var item in items)
                {
                    if (!await _context.Products.AnyAsync(p => p.Id == item.ProductId))
                        throw new InvalidOperationException($"product {item.ProductId} not found");
                }

                var order = new Order
                {
                    UserId = userId,
                    AddressesId = addressId, 
                    PaymentMethod = paymentMethod,
                    ShippingCost = shippingCost, 
                    CreatedAt = DateTime.Now, 
                    Status = OrderStatus.Pending,
                    OrderItems = new List<OrderItem>()
                };

                await _context.Orders.AddAsync(order);
                await _context.SaveChangesAsync();

                foreach (var item in items)
                {
                    var orderItem = new OrderItem
                    {
                        productId = item.ProductId,
                        OrderId = order.Id,
                        Amount = item.Quantity,
                        Price = item.Product.Price
                    };
                    order.OrderItems.Add(orderItem);
                    await _context.OrderItems.AddAsync(orderItem);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<Order>> GetallOrders(string userId)
        {
            var orders = await _context.Orders.Where(o=>o.UserId==userId).ToListAsync();
            return orders;
        }

        public async Task<Dictionary<int, List<OrderItem>>> GetorderItems(string userId)
            => await _context.OrderItems
                    .Include(oi => oi.Product)
                    .Include(oi => oi.Order)
                    .Include(oi => oi.Order.ShippingAddress)
                    .ThenInclude(o => o.User)
                    .Where(oi => oi.Order.UserId == userId)
                    .GroupBy(oi => oi.OrderId)
                    .ToDictionaryAsync(g => g.Key, g => g.ToList());

       
        public async Task<Dictionary<int, List<OrderItem>>> Getall()
        => await _context.OrderItems
                .Include(oi => oi.Product)
                .Include(oi => oi.Order)
                .ThenInclude(o => o.User)
                .Include(i => i.Order.ShippingAddress)
                .GroupBy(oi => oi.OrderId)
                .ToDictionaryAsync(g => g.Key, g => g.ToList());
       
        public async Task UpdateOrderStatus(int orderId, OrderStatus status)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null)
            {
                order.Status = status;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Order> GetOrderWithUserDetails(int orderId)
        {
            return await _context.Orders
                         .Include(o => o.User)
                         .Include(o => o.ShippingAddress) // تضمين العنوان
                         .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                         .FirstOrDefaultAsync(o => o.Id == orderId);
        }

    }
}
