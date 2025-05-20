using Microsoft.AspNetCore.Mvc;
using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using TechXpress_DepiGraduation.Data.ViewModel; 

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
        public async Task<IActionResult> Index(ContactUsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var apiKey = _configuration["SendGridApiKey"];
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(_configuration["EmailFrom"]);
            var to = new EmailAddress(_configuration["EmailTo"]);
            var subject = model.Subject; 
            var replyTo = new EmailAddress(model.Email, model.Name); 

            var plainTextContent = $"Message from: {model.Name} ({model.Email})\n\n{model.Message}";
            var htmlContent = $"<strong>Message from:</strong> {model.Name} ({model.Email})<br><br>{model.Message}";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            msg.ReplyTo = replyTo; 

            var response = await client.SendEmailAsync(msg);

            if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
            {
                TempData["Message"] = "Thank you for your message! We'll get back to you soon.";
            }
            else
            {
                TempData["Message"] = "Failed to send your message. Please try again later.";
            }

            return RedirectToAction("Index");
        }
    }
}