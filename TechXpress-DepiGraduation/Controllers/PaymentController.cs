using System.Collections;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechXpress_DepiGraduation.Data.Services;
using TechXpress_DepiGraduation.Data.Cart;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;
using TechXpress_DepiGraduation.Models;

namespace TechXpress_DepiGraduation.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;
        private readonly ShoppingCart _shoppingCart;

        public PaymentController(IPaymentService paymentService, IOrderService orderService, ShoppingCart shoppingCart)
        {
            _paymentService = paymentService;
            _orderService = orderService;
            _shoppingCart = shoppingCart;
        }

        [HttpGet]
        [Authorize]
        public IActionResult SelectPayment()
        {
            var paymentMethods = new List<string> { "Credit Card", "PayPal", "Cash on Delivery" };
            ViewBag.PaymentMethods = paymentMethods;
            
            return View();
        }

        [HttpPost]
        [Authorize]
        public IActionResult SelectPayment(string paymentMethod)
        {
            if (string.IsNullOrEmpty(paymentMethod))
            {
                TempData["Error"] = "Please select a payment method.";
                return View();
            }

            TempData["PaymentMethod"] = paymentMethod;
            return RedirectToAction("EnterAddress");
        }
        [HttpGet]
        [Authorize]
        public IActionResult EnterAddress()
        {
            var userId = TempData["UserId"]?.ToString();

            ViewBag.Addresses =  _paymentService.getAddressesbyUserID(userId) ;
            
           
            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> EnterAddress([Bind("Street,City,Country")]AddressView address)
        {
            
            var cartItemsJson = TempData["CartItems"]?.ToString();
            
            var userId =User.FindFirstValue(ClaimTypes.NameIdentifier).ToString();
            
            
            var paymentMethod = TempData["PaymentMethod"]?.ToString();
            

            // if ( string.IsNullOrEmpty(cartItemsJson) )
            // {
            //     TempData["Error"] = "Session expired. Please try again.";
            //     return RedirectToAction("Index", "Order");
            // }
            var cartItems = System.Text.Json.JsonSerializer.Deserialize<List<ShoppingCartItem>>(cartItemsJson);

            try
            {
                await _orderService.StoreOrdersAsync(cartItems, userId);
                await _shoppingCart.Makecartempty();

                // Clear TempData
                TempData.Clear();

                // Redirect to Order Completed view
                return RedirectToAction("OrderCompleted","Order");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while processing your order. Please try again.";
                return RedirectToAction("Index", "Order");
            }
        }
    }
}