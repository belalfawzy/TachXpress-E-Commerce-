using PayPalCheckoutSdk.Core;
using PayPalCheckoutSdk.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace TechXpress_DepiGraduation.Data.Services
{
    public class PaypalService
    {
        private readonly PayPalHttpClient _client;
        private readonly IConfiguration _configuration;

        // Model for PayPal token response
        private class TokenResponse
        {
            public string access_token { get; set; }
            public string token_type { get; set; }
            public string app_id { get; set; }
            public int expires_in { get; set; }
            public string nonce { get; set; }
        }

        // Model for PayPal capture response
        private class CaptureResponse
        {
            public string id { get; set; }
            public string status { get; set; }
        }

        public PaypalService(IConfiguration configuration)
        {
            _configuration = configuration;
            var mode = _configuration["PayPal:mode"] ?? "sandbox";
            var clientId = _configuration["PayPal:clientId"] ?? throw new ArgumentNullException("PayPal:clientId");
            var clientSecret = _configuration["PayPal:clientSecret"] ?? throw new ArgumentNullException("PayPal:clientSecret");
            var environment = new SandboxEnvironment(clientId, clientSecret);
            _client = new PayPalHttpClient(environment);
            Console.WriteLine($"PaypalService initialized: ClientId={clientId}, Mode={mode}");
        }

        public async Task<(string ApprovalUrl, string PaymentId)> CreatePayment(decimal amount, string currency, string returnUrl, string cancelUrl)
        {
            var order = new OrderRequest
            {
                CheckoutPaymentIntent = "CAPTURE",
                
                PurchaseUnits = new List<PurchaseUnitRequest>
                {
                    new PurchaseUnitRequest
                    {
                        AmountWithBreakdown = new AmountWithBreakdown
                        {
                            CurrencyCode = currency,
                            Value = amount.ToString("F2"),
                            AmountBreakdown = new AmountBreakdown
                            {
                                ItemTotal = new Money
                                {
                                    CurrencyCode = currency,
                                    Value = amount.ToString("F2")
                                }
                            }
                        },
                        Description = "Purchase of digital goods",
                        Items = new List<Item>
                        {
                            new Item
                            {
                                Name = "Test Item",
                                Description = "Digital product for testing",
                                Quantity = "1",
                                UnitAmount = new Money
                                {
                                    CurrencyCode = currency,
                                    Value = amount.ToString("F2")
                                }
                            }
                        }
                    }
                },
                ApplicationContext = new ApplicationContext
                {
                    ReturnUrl = returnUrl,
                    CancelUrl = cancelUrl
                }
            };

            var request = new OrdersCreateRequest();
            request.Prefer("return=representation");
            request.RequestBody(order);

            try
            {
                Console.WriteLine($"CreatePayment Request: Amount={amount}, Currency={currency}, Description={order.PurchaseUnits[0].Description}");
                var response = await _client.Execute(request);
                var result = response.Result<Order>();
                Console.WriteLine($"CreatePayment: OrderId={result.Id}, Status={result.Status}");
                var approvalUrl = result.Links.FirstOrDefault(link => link.Rel.Equals("approve", StringComparison.OrdinalIgnoreCase))?.Href;

                if (string.IsNullOrEmpty(approvalUrl))
                    throw new Exception("No approval URL found.");

                return (approvalUrl, result.Id);
            }
            catch (PayPalHttp.HttpException ex)
            {
                Console.WriteLine($"CreatePayment HttpException: Status={ex.StatusCode}, Message={ex.Message}");
                throw new Exception($"Failed to create PayPal payment: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CreatePayment Error: {ex.Message}, Inner: {ex.InnerException?.Message}");
                throw;
            }
        }

        public async Task<bool> ExecutePayment(string payerId, string paymentId)
        {
            if (string.IsNullOrEmpty(paymentId) || string.IsNullOrEmpty(payerId))
            {
                Console.WriteLine($"ExecutePayment Error: Invalid inputs - paymentId={paymentId}, payerId={payerId}");
                throw new ArgumentException("Payment ID or Payer ID is missing.");
            }

            try
            {
                // Check order status before capturing
                var statusRequest = new OrdersGetRequest(paymentId);
                var statusResponse = await _client.Execute(statusRequest);
                var order = statusResponse.Result<Order>();
                Console.WriteLine($"Pre-Capture Order Status: {order.Status}");
                if (order.Status != "APPROVED")
                {
                    Console.WriteLine($"ExecutePayment Failed: Order status is {order.Status}, expected APPROVED.");
                    return false;
                }

                Console.WriteLine($"Executing payment: paymentId={paymentId}, payerId={payerId}");
                var (success, status) = await ManualCapture(paymentId);
                Console.WriteLine($"ExecutePayment Response via ManualCapture: Success={success}, Status={status}");
                if (!success)
                {
                    Console.WriteLine($"ExecutePayment Failed: Capture request was not successful.");
                    return false;
                }
                if (status != "COMPLETED")
                {
                    Console.WriteLine($"ExecutePayment Failed: Order status is {status}, expected COMPLETED.");
                    return false;
                }
                return true;
            }
            catch (PayPalHttp.HttpException ex)
            {
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ExecutePayment Error: {ex.Message}, Inner: {ex.InnerException?.Message}");
                throw new Exception($"Failed to capture PayPal payment: {ex.Message}", ex);
            }
        }

        public async Task<(bool Success, string Status)> ManualCapture(string paymentId)
        {
            try
            {
                var request = new OrdersCaptureRequest(paymentId);
                request.Prefer("return=representation");
                request.RequestBody(new OrderActionRequest());

                Console.WriteLine($"CaptureOrder Request: paymentId={paymentId}");
                var response = await _client.Execute(request);
                var result = response.Result<Order>();
                Console.WriteLine($"CaptureOrder Response: StatusCode={response.StatusCode}, OrderStatus={result.Status}");

                // if (response.StatusCode < 200 || response.StatusCode >= 300)
                // {
                //     Console.WriteLine($"CaptureOrder Failed: StatusCode={response.StatusCode}, OrderStatus={result.Status}");
                //     return (false, "FAILED");
                // }

                return (true, result.Status ?? "UNKNOWN");
            }
            catch (PayPalHttp.HttpException ex)
            {
                return (false, "FAILED");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CaptureOrder Error: {ex.Message}, Inner: {ex.InnerException?.Message}");
                throw new Exception($"Failed to capture PayPal order: {ex.Message}", ex);
            }
        }    }
}