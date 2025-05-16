using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechXpress_DepiGraduation.Models;

namespace TechXpress_DepiGraduation.Controllers
{
    public class AddressesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        public AddressesController(AppDbContext context,UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        [HttpPost]
        public async Task<IActionResult> AddAddress([FromBody]Addresses address)
        {
            var userId = _userManager.GetUserId(User);
            address.UserId = userId;

            _context.Addresses.Add(address);
            _context.SaveChanges();

            return Json(new { success = true });
        }
    }
}
