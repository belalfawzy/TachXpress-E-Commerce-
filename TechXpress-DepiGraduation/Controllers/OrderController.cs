using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechXpress_DepiGraduation.Data.Cart;
using TechXpress_DepiGraduation.Data.Enums;
using TechXpress_DepiGraduation.Data.Services;
using TechXpress_DepiGraduation.Data.ViewModel;
using TechXpress_DepiGraduation.Models;

namespace TechXpress_DepiGraduation.Controllers
{
    public class OrderController : Controller
    {
        private readonly IProductService _productService;
        private readonly ShoppingCart _shoppingCart;
        private readonly IOrderService _orderService;
        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public OrderController(IConfiguration configuration,IProductService productService, ShoppingCart shoppingCart, IOrderService orderService, AppDbContext context, UserManager<AppUser> userManager)
        {
            _productService = productService;
            _shoppingCart = shoppingCart;
            _orderService = orderService;
            _configuration = configuration;
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            ViewBag.CartId = _shoppingCart.ShoppingCartId;
            var items = _shoppingCart.GetShoppingCartItems();
            ViewBag.Total = _shoppingCart.GetTotal();
            return View(items);
        }

        public async Task<IActionResult> AddToCart(int Id)
        {
            var item = await _productService.GetItemByIdAsync(Id);
            if (item != null)
            {
                await _shoppingCart.AddItemToCart(item);
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    var count = _shoppingCart.GetShoppingCartItemsCount();
                    var itemQuantity = _shoppingCart.GetItemQuantity(item.Id);
                    return Json(new
                    {
                        success = true,
                        count = count,
                        productId = item.Id,
                        quantity = itemQuantity
                    });
                }
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> DecreaseFromCart(int Id)
        {
            var item = await _productService.GetItemByIdAsync(Id);
            if (item != null)
            {
                await _shoppingCart.RemoveItemFromCart(item);
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    var count = _shoppingCart.GetShoppingCartItemsCount();
                    var itemQuantity = _shoppingCart.GetItemQuantity(item.Id);
                    return Json(new
                    {
                        success = true,
                        count = count,
                        productId = item.Id,
                        quantity = itemQuantity
                    });
                }
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> DeleteItemFromCart(int Id)
        {
            await _shoppingCart.DeleteItem(Id);
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var count = _shoppingCart.GetShoppingCartItemsCount();
                return Json(new
                {
                    success = true,
                    count = count,
                    productId = Id,
                    quantity = 0
                });
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult GetCartCount()
        {
            var count = _shoppingCart.GetShoppingCartItemsCount();
            return Json(new { count = count });
        }

        [Authorize]
        public async Task<IActionResult> OrderCompleted()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> ListAllOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = User.IsInRole("Admin");

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            if (!role)
            {
                var orders = await _orderService.GetorderItems(userId);
                return View(orders);
            }
            else
            {
                var orders = await _orderService.Getall();
                return View(orders);
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var addresses = await _context.Addresses
                .Where(a => a.UserId == userId)
                .ToListAsync();

            var cartItems = _shoppingCart.GetShoppingCartItems();
            var total = _shoppingCart.GetTotal();

            if (!cartItems.Any())
            {
                return RedirectToAction("Index");
            }

            var viewModel = new OrderCheckoutViewModel
            {
                UserAddresses = addresses,
                CartItems = cartItems,
                Total = total,
                NewAddress = new Addresses()
            };
            ViewBag.PayPalClientId = _configuration["PayPal:clientId"];
            ViewBag.CartItems = cartItems;
            ViewBag.Total = total;
            

            return View(viewModel);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> PlaceOrder(int addressId, PaymentMethod paymentMethod, decimal shippingCost, string paypalEmail = null)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var cartItems = _shoppingCart.GetShoppingCartItems();

                if (!cartItems.Any())
                {
                    return Json(new { success = false, message = "Shopping cart is empty!" });
                }

                var address = await _context.Addresses
                    .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId);

                if (address == null)
                {
                    return Json(new { success = false, message = "Invalid Address" });
                }

                // Calculate total amount
                decimal totalAmount = cartItems.Sum(item => item.Product.Price * item.Quantity) + shippingCost;

                // Store order
                await _orderService.StoreOrdersAsync(cartItems, userId, addressId, paymentMethod, shippingCost, paypalEmail);

                // Clear cart
                await _shoppingCart.Makecartempty();

                if (paymentMethod == PaymentMethod.PayPal)
                {
                    var redirectUrl = Url.Action("PaymentWithPaypal", "PayPal", new
                    {
                        amount = totalAmount,
                        currency = "USD",
                        paypalEmail = paypalEmail
                    }, Request.Scheme);

                    return Json(new { success = true, redirectUrl });
                }

                return Json(new
                {
                    success = true,
                    redirectUrl = Url.Action("OrderCompleted", "Order", null, Request.Scheme)
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error Occurred: " + ex.Message });
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var order = await _orderService.GetOrderWithUserDetails(orderId);

            if (order != null && order.UserId == userId && order.Status == OrderStatus.Pending)
            {
                await _orderService.UpdateOrderStatus(orderId, OrderStatus.Cancelled);
                TempData["Success"] = "Order has been cancelled successfully.";
            }
            else
            {
                TempData["Error"] = "Unable to cancel order. It may have already been processed.";
            }

            return RedirectToAction(nameof(ListAllOrders));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, OrderStatus status)
        {
            var order = await _orderService.GetOrderWithUserDetails(orderId);
            if (order == null)
            {
                return NotFound();
            }

            await _orderService.UpdateOrderStatus(orderId, status);
            return RedirectToAction(nameof(ListAllOrders));
        }
    }
}