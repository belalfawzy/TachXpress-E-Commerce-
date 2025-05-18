using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
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
        public async Task<IActionResult> PaymentWithPaypal(decimal amount, string currency = "USD", string paypalEmail = "")
        {
            try
            {
                // Validate PayPal email (optional, as not used in CreatePayment)
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
                var returnUrl = Url.Action("ExecutePaypalPayment", "PayPal", null, Request.Scheme);
                var cancelUrl = Url.Action("Checkout", "Order", null, Request.Scheme);
                var approvalUrl = await _payPalService.CreatePayment(amount, currency, returnUrl, cancelUrl);

                if (!string.IsNullOrEmpty(approvalUrl))
                {
                    Console.WriteLine($"PaymentWithPaypal: Approval URL={approvalUrl}");
                    return Json(new { success = true, approvalUrl });
                }

                return Json(new { success = false, message = "Failed to create PayPal payment" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PaymentWithPaypal Error: {ex.Message}, Inner: {ex.InnerException?.Message}");
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExecutePaypalPayment(string PayerID, string PaymentId)
        {
            try
            {
                 Console.WriteLine($"ExecutePaypalPayment: PaymentId={PaymentId}, PayerID={PayerID}");
                if (string.IsNullOrEmpty(PayerID) || string.IsNullOrEmpty(PaymentId))
                {
                    TempData["Error"] = "Invalid payment details";
                    return RedirectToAction("Checkout", "Order");
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                Console.WriteLine($"UserId: {userId}");
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["Error"] = "User not authenticated";
                    return RedirectToAction("Checkout", "Order");
                }

                // Execute payment
                var isApproved = await _payPalService.ExecutePayment(PayerID, PaymentId);
                Console.WriteLine($"Payment Approved: {isApproved}");

                if (isApproved)
                {
                    return RedirectToAction("OrderCompleted", "Order");
                }

                TempData["Error"] = "Payment was not approved";
                return RedirectToAction("Checkout", "Order");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ExecutePaypalPayment Error: {ex.Message}, Inner: {ex.InnerException?.Message}");
                TempData["Error"] = $"Payment error: {ex.Message}";
                return RedirectToAction("Checkout", "Order");
            }
        }
    }
}