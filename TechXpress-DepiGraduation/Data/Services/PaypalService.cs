using PayPalCheckoutSdk.Core;
using PayPalCheckoutSdk.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechXpress_DepiGraduation.Data.Services
{
    public class PaypalService
    {
        private readonly PayPalHttpClient _client;
        private readonly IConfiguration _configuration;

        public PaypalService(IConfiguration configuration)
        {
            _configuration = configuration;
            var mode = _configuration["PayPal:mode"] ?? "sandbox";
            var clientId = _configuration["PayPal:clientId"] ?? throw new ArgumentNullException("PayPal:clientId");
            var clientSecret = _configuration["PayPal:clientSecret"] ?? throw new ArgumentNullException("PayPal:clientSecret");
            var environment = new SandboxEnvironment(clientId, clientSecret) ;
            _client = new PayPalHttpClient(environment);
            Console.WriteLine($"PaypalService initialized: ClientId={clientId}, Mode={mode}");
        }

        public async Task<string> CreatePayment(decimal amount, string currency, string returnUrl, string cancelUrl)
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
                            Value = amount.ToString("F2")
                        },
                        Description = "Sandbox Test Purchase"
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
                return approvalUrl ?? throw new Exception("No approval URL found.");
                
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
            // Validate inputs
            if (string.IsNullOrEmpty(paymentId) || string.IsNullOrEmpty(payerId))
            {
                Console.WriteLine($"ExecutePayment Error: Invalid inputs - paymentId={paymentId}, payer_id={payerId}");
                throw new ArgumentException("Payment ID or Payer ID is missing.");
            }

            // Verify order state
            try
            {
                var getRequest = new OrdersGetRequest(paymentId);
                var getResponse = await _client.Execute(getRequest);
                var order = getResponse.Result<Order>();
                Console.WriteLine($"Order State: {order.Status}, PayerId={order.Payer?.PayerId}");
                if (order.Status != "APPROVED")
                {
                    Console.WriteLine($"ExecutePayment Error: Order not in APPROVED state - State={order.Status}");
                    throw new InvalidOperationException($"Order is not approved. State: {order.Status}");
                }
                if (order.Payer?.PayerId != payerId)
                {
                    Console.WriteLine($"ExecutePayment Error: Payer ID mismatch - Expected={order.Payer?.PayerId}, Received={payerId}");
                    throw new InvalidOperationException("Payer ID does not match order.");
                }
            }
            catch (PayPalHttp.HttpException ex)
            {
                Console.WriteLine($"Order Get Error: Status={ex.StatusCode}, Message={ex.Message}");
                throw;
            }

            var request = new OrdersCaptureRequest(paymentId);
            request.Prefer("return=representation");

            try
            {
                Console.WriteLine($"Executing payment: paymentId={paymentId}, payer_id={payerId}");
                var response = await _client.Execute(request);
                var result = response.Result<Order>();
                Console.WriteLine($"ExecutePayment Response: Status={result.Status}, PayerId={result.Payer?.PayerId}");
                return result.Status == "COMPLETED" && result.Payer?.PayerId == payerId;
            }
            catch (PayPalHttp.HttpException ex)
            {
                Console.WriteLine($"ExecutePayment HttpException: Status={ex.StatusCode}, Message={ex.Message}");
                throw new Exception($"Failed to capture PayPal payment: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ExecutePayment Error: {ex.Message}, Inner: {ex.InnerException?.Message}");
                throw;
            }
        }
    }
}