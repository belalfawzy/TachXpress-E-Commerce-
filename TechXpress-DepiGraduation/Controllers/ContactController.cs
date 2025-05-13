
using Microsoft.AspNetCore.Mvc;
using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace TechXpress_DepiGraduation.Controllers
{
    public class ContactController : Controller
    {
        private readonly IConfiguration _configuration;


        public ContactController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string name, string email, string subject, string message)
        {
            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(message))
            {
                var apiKey = _configuration["SendGridApiKey"];
                var client = new SendGridClient(apiKey);
                var from = new EmailAddress(_configuration["EmailFrom"]);
                var to = new EmailAddress(_configuration["EmailTo"]);
                var msg = MailHelper.CreateSingleEmail(from, to, subject, $"{name} ({email})", message);
                var response = await client.SendEmailAsync(msg);

                if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
                {
                    TempData["Message"] = "Thank you for your message! We'll get back to you soon.";
                }
                else
                {
                    TempData["Message"] = "Failed to send your message. Please try again later.";
                }
            }
            else
            {
                TempData["Message"] = "Please fill in all required fields.";
            }

            return RedirectToAction("Index");
        }
    }
}