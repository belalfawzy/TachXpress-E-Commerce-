using Microsoft.AspNetCore.Http.HttpResults;
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
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (items == null || !items.Any())
                    throw new ArgumentException("Shopping cart items cannot be null or empty.");

                foreach (var item in items)
                {
                    if (!await _context.Products.AnyAsync(p => p.Id == item.ProductId))
                        throw new InvalidOperationException($"Product with Id {item.ProductId} does not exist.");
                }

                var order = new Order
                {
                    UserId = userId,
                    PaymentStatus = "Pending",
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
                    await _context.OrderItems.AddAsync(orderItem); // Add to context
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
        {
            var orderwithitems = await _context.Orders.Where(o => o.UserId
                                                                  == userId).Join(_context.OrderItems, o => o.Id,
                    ot => ot.OrderId, (o, ot) => new { OrderId = o.Id, OrderItem = ot })
                .GroupBy(o => o.OrderId)
                .Select(v => new KeyValuePair<int, List<OrderItem>>(v.Key, v.Select(x => x.OrderItem).ToList()))
                .ToDictionaryAsync(d => d.Key, d => d.Value);
            foreach (var item in orderwithitems)
            {
                foreach(var val in item.Value)
                {
                    val.Product = _context.Products.Where(p => p.Id == val.productId).FirstOrDefault();

                }
            }
            return orderwithitems;
        }
        public async Task<Dictionary<int, List<OrderItem>>> Getall()
        {
            var orderwithitems = await _context.Orders.Join(_context.OrderItems, o => o.Id,
                    ot => ot.OrderId, (o, ot) => new { OrderId = o.Id, OrderItem = ot })
                .GroupBy(o => o.OrderId)
                .Select(v => new KeyValuePair<int, List<OrderItem>>(v.Key, v.Select(x => x.OrderItem).ToList()))
                .ToDictionaryAsync(d => d.Key, d => d.Value);

           
            return orderwithitems;
        }

    }
}
