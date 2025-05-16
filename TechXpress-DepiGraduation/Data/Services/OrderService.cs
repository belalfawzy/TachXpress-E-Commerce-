using Microsoft.EntityFrameworkCore;
using TechXpress_DepiGraduation.Data.Enums;
using TechXpress_DepiGraduation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechXpress_DepiGraduation.Data.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;

        public OrderService(AppDbContext context)
        {
            _context = context;
        }

        public async Task StoreOrdersAsync(List<ShoppingCartItem> items, string userId, int addressId, PaymentMethod paymentMethod, decimal shippingCost, string paypalEmail = null)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (items == null || !items.Any())
                    throw new ArgumentException("The Cart is Empty");

                var address = await _context.Addresses.FindAsync(addressId);
                if (address == null || address.UserId != userId)
                    throw new InvalidOperationException("Invalid Address");

                foreach (var item in items)
                {
                    if (!await _context.Products.AnyAsync(p => p.Id == item.ProductId))
                        throw new InvalidOperationException($"Product {item.ProductId} not found");
                }

                var order = new Order
                {
                    UserId = userId,
                    AddressesId = addressId,
                    PaymentMethod = paymentMethod,
                    ShippingCost = shippingCost,
                    CreatedAt = DateTime.Now,
                    Status = OrderStatus.Pending,
                 //   PaypalEmail = paypalEmail, // Store PayPal email if provided
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

        public Task<List<Order>> GetOrderAndRoleByUserIdAsync(string userId, string role)
        {
            return _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(u => u.UserId == userId)
                .ToListAsync();
        }

        public async Task<List<Order>> GetallOrders(string userId)
        {
            return await _context.Orders.Where(o => o.UserId == userId).ToListAsync();
        }

        public async Task<Dictionary<int, List<OrderItem>>> GetorderItems(string userId)
        {
            return await _context.OrderItems
                .Include(oi => oi.Product)
                .Include(oi => oi.Order)
                .Include(oi => oi.Order.ShippingAddress)
                .ThenInclude(o => o.User)
                .Where(oi => oi.Order.UserId == userId)
                .GroupBy(oi => oi.OrderId)
                .ToDictionaryAsync(g => g.Key, g => g.ToList());
        }

        public async Task<Dictionary<int, List<OrderItem>>> Getall()
        {
            return await _context.OrderItems
                .Include(oi => oi.Product)
                .Include(oi => oi.Order)
                .ThenInclude(o => o.User)
                .Include(i => i.Order.ShippingAddress)
                .GroupBy(oi => oi.OrderId)
                .ToDictionaryAsync(g => g.Key, g => g.ToList());
        }

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
                .Include(o => o.ShippingAddress)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }
    }
}