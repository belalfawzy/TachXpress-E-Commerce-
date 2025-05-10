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
        public OrderController(IProductService productService, ShoppingCart shoppingCart,IOrderService orderService,AppDbContext context)
        {
            _productService = productService;
            _shoppingCart = shoppingCart;
            _orderService = orderService;
            _context = context;
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
        [HttpGet]
        public IActionResult GetCartCount()
        {
            var count = _shoppingCart.GetShoppingCartItemsCount();
            return Json(new { count = count });
        }



        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CompleteOrder()
        {
            var cartItems = _shoppingCart.GetShoppingCartItems();
            if (!cartItems.Any())
            {
                TempData["Error"] = "Your cart is empty. Please add items before checking out.";
                return RedirectToAction(nameof(Index));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            
            
            TempData["CartItems"] = System.Text.Json.JsonSerializer.Serialize(cartItems);
            TempData["UserId"] = userId.ToString();

            return RedirectToAction("SelectPayment", "Payment");
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
            { var  orders = await _orderService.GetorderItems(userId);
                
                return View(orders);

            }
            else
            {
               var  orders = await _orderService.Getall();
               return View(orders);

            }
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Get user addresses
            var addresses = await _context.Addresses
                .Where(a => a.UserId == userId)
                .ToListAsync();

            // Get cart items
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
                Total = total
            };
            ViewBag.CartItems = cartItems;
            ViewBag.Total = total;

            return View(viewModel);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> PlaceOrder(int addressId, PaymentMethod paymentMethod)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cartItems = _shoppingCart.GetShoppingCartItems();

            if (!cartItems.Any())
            {
                return Json(new { success = false, message = "Your cart is empty." });
            }

            // Get selected address
            var address = await _context.Addresses
                .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId);

            if (address == null)
            {
                return Json(new { success = false, message = "Invalid shipping address." });
            }

            // Store order
            await _orderService.StoreOrdersAsync(cartItems, userId);

            // Clear shopping cart
            await _shoppingCart.Makecartempty();

            if (paymentMethod == PaymentMethod.CashOnDelivery)
            {
                // Implement PayPal integration here
                // For now, just return a dummy URL
                return Json(new
                {
                    success = true,
                    redirectUrl = Url.Action("OrderCompleted", "Order")
                });
            }

            return Json(new
            {
                success = true,
                redirectUrl = Url.Action("OrderCompleted", "Order")
            });
        }
        public IActionResult OrderCompleted()
        {
            return View(); 
        }
    }
}
