using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechXpress_DepiGraduation.Data.Cart;
using TechXpress_DepiGraduation.Data.Services;
using TechXpress_DepiGraduation.Models;

namespace TechXpress_DepiGraduation.Controllers
{
    public class OrderController : Controller
    {
        private readonly IProductService _productService;
        private readonly ShoppingCart _shoppingCart;
        private readonly IOrderService _orderService;

        public OrderController(IProductService productService, ShoppingCart shoppingCart,IOrderService orderService)
        {
            _productService = productService;
            _shoppingCart = shoppingCart;
            _orderService = orderService;

        }
        public IActionResult Index()
        {
            ViewBag.CartId = _shoppingCart.ShoppingCartId;

            var items = _shoppingCart.GetShoppingCartItems();
            ViewBag.Total = _shoppingCart.GetTotal();
            return View(items);
        }
        public async Task<IActionResult> AddToCart( int Id)
        {
            var item = await _productService.GetItemByIdAsync(Id);
            if (item != null)
            {
                await _shoppingCart.AddItemToCart(item);
            }

            return RedirectToAction(nameof(Index));
        }

       
        public async Task<IActionResult> DecreaseFromCart(int Id)
        {
            var item = await _productService.GetItemByIdAsync(Id);
            if (item != null)
            {
                await _shoppingCart.RemoveItemFromCart(item);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> DeleteItemFromCart(int Id)
        {
            
            await _shoppingCart.DeleteItem(Id);
            return RedirectToAction(nameof(Index));
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

        public IActionResult OrderCompleted()
        {
            return View(); 
        }
        
        
        
        


    }
}
