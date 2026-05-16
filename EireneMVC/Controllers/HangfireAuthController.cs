using Eirene.DAL.Entities.Core;
using EireneMVC.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EireneMVC.Controllers
{
    [Route("hangfire-login")]
    public class HangfireAuthController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public HangfireAuthController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Login([FromQuery] string? returnUrl = null)
        {
            return View(new HangfireLoginViewModel { ReturnUrl = returnUrl ?? "/hangfire" });
        }

        [HttpPost]
        public async Task<IActionResult> Login(HangfireLoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                if (await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.UserName ?? user.Email ?? string.Empty),
                        new Claim(ClaimTypes.Role, "Admin")
                    };

                    var identity = new ClaimsIdentity(claims, "HangfireCookie");
                    var principal = new ClaimsPrincipal(identity);

                    await HttpContext.SignInAsync("HangfireCookie", principal);

                    return Redirect(model.ReturnUrl ?? "/hangfire");
                }
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt or insufficient permissions.");
            return View(model);
        }
    }
}
