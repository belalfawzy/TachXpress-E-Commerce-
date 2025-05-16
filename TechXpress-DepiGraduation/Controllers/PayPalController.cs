using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using TechXpress_DepiGraduation.Data.Services;

namespace TechXpress_DepiGraduation.Controllers
{
    public class PayPalController : Controller
    {
        private readonly PaypalService _payPalService;

        public PayPalController(PaypalService payPalService)
        {
            _payPalService = payPalService;
        }

        [HttpPost]
        public IActionResult PaymentWithPaypal(decimal amount, string currency = "USD", string paypalEmail = "")
        {
            try
            {
                // Validate PayPal email
                if (string.IsNullOrWhiteSpace(paypalEmail) || !paypalEmail.Contains("@"))
                {
                    return Json(new { success = false, message = "Invalid PayPal email address" });
                }

                // Get user ID from Identity
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                // Create payment
                var returnUrl = Url.Action("Checkout", "Order", null, Request.Scheme);
                var cancelUrl = Url.Action("Checkout", "Order", null, Request.Scheme);
                
                var approvalUrl = _payPalService.CreatePayment(amount, currency, returnUrl, cancelUrl);

                if (approvalUrl != null)
                {
                    return Json(new { success = true, approvalUrl });
                }

                return Json(new { success = false, message = "Failed to create PayPal payment" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PaymentWithPaypal Error: {ex.Message}");
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpPost]
        public IActionResult ExecutePaypalPayment(string payerId, string paymentId)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrEmpty(payerId) || string.IsNullOrEmpty(paymentId))
                {
                    return Json(new { success = false, message = "Invalid payment details" });
                }

                // Get user ID from Identity
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                // Execute payment
                var isApproved = _payPalService.ExecutePayment(payerId, paymentId);

                if (isApproved)
                {
                    return Json(new { success = true, message = "Payment completed successfully" });
                }

                return Json(new { success = false, message = "Payment was not approved" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ExecutePaypalPayment Error: {ex.Message}");
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }
    }
}