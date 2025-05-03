using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;
using TechXpress_DepiGraduation.Data.StaticMembers;
using TechXpress_DepiGraduation.Data.ViewModel;
using TechXpress_DepiGraduation.Models;

namespace TechXpress_DepiGraduation.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        public AccountController(AppDbContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }
        public IActionResult Login()
        {
            var res = new LoginViewModel();
            return View(res);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel loginViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(loginViewModel);
            }
            var user = await _userManager.FindByEmailAsync(loginViewModel.Email);
            if (user != null)
            {
                var result = await _signInManager.PasswordSignInAsync(user, loginViewModel.Password, isPersistent: false, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            ModelState.AddModelError("", "Invalid Email or password");
            return View(loginViewModel);
        }
        public IActionResult Register()
        {
            var res = new RegisterViewModel();
            return View(res);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel registerViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(registerViewModel);
            }
            var user = new AppUser()
            {
                Email = registerViewModel.Email,
                FullName = registerViewModel.FullName,
                UserName = registerViewModel.Email.Split('@')[0],
                PhoneNumber = registerViewModel.PhoneNumber,
            };
            var result = await _userManager.CreateAsync(user, registerViewModel.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, UserRoles.User);
                return View("RegisterationComplete");
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return View("registerViewModel");
        }
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
        public async Task<IActionResult> Users()
        {
            var users = _userManager.Users.ToList();
            var userRoles = new Dictionary<string, List<string>>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles[user.Id] = roles.ToList();
            }

            ViewBag.UserRoles = userRoles;

            return View(users);
        }
    }
}
