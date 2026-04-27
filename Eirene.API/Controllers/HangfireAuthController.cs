using Eirene.DAL.Entities.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Eirene.API.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
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
            var html = @"
                <!DOCTYPE html>
                <html>
                <head>
                    <title>Hangfire Dashboard Login</title>
                    <style>
                        body { font-family: Arial, sans-serif; display: flex; justify-content: center; align-items: center; height: 100vh; margin: 0; background-color: #f4f6f8; }
                        .login-container { background: #fff; padding: 30px; border-radius: 8px; box-shadow: 0 4px 12px rgba(0,0,0,0.1); width: 100%; max-width: 400px; }
                        h2 { margin-top: 0; color: #333; text-align: center; }
                        .form-group { margin-bottom: 15px; }
                        label { display: block; margin-bottom: 5px; color: #666; }
                        input[type='text'], input[type='password'] { width: 100%; padding: 10px; border: 1px solid #ccc; border-radius: 4px; box-sizing: border-box; }
                        button { width: 100%; padding: 10px; background-color: #007bff; color: white; border: none; border-radius: 4px; cursor: pointer; font-size: 16px; }
                        button:hover { background-color: #0056b3; }
                        .error { color: red; margin-bottom: 15px; text-align: center; }
                    </style>
                </head>
                <body>
                    <div class='login-container'>
                        <h2>Hangfire Admin</h2>
                        <form method='post' action='/hangfire-login'>
                            <input type='hidden' name='returnUrl' value='" + (returnUrl ?? "/hangfire") + @"' />
                            <div class='form-group'>
                                <label for='email'>Email</label>
                                <input type='text' id='email' name='email' required />
                            </div>
                            <div class='form-group'>
                                <label for='password'>Password</label>
                                <input type='password' id='password' name='password' required />
                            </div>
                            <button type='submit'>Login</button>
                        </form>
                    </div>
                </body>
                </html>";

            return Content(html, "text/html");
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password, [FromForm] string? returnUrl = null)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null && await _userManager.CheckPasswordAsync(user, password))
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

                    return Redirect(returnUrl ?? "/hangfire");
                }
            }

            var errorHtml = @"
                <!DOCTYPE html>
                <html>
                <head>
                    <title>Hangfire Dashboard Login</title>
                    <style>
                        body { font-family: Arial, sans-serif; display: flex; justify-content: center; align-items: center; height: 100vh; margin: 0; background-color: #f4f6f8; }
                        .login-container { background: #fff; padding: 30px; border-radius: 8px; box-shadow: 0 4px 12px rgba(0,0,0,0.1); width: 100%; max-width: 400px; }
                        h2 { margin-top: 0; color: #333; text-align: center; }
                        .form-group { margin-bottom: 15px; }
                        label { display: block; margin-bottom: 5px; color: #666; }
                        input[type='text'], input[type='password'] { width: 100%; padding: 10px; border: 1px solid #ccc; border-radius: 4px; box-sizing: border-box; }
                        button { width: 100%; padding: 10px; background-color: #007bff; color: white; border: none; border-radius: 4px; cursor: pointer; font-size: 16px; }
                        button:hover { background-color: #0056b3; }
                        .error { color: red; margin-bottom: 15px; text-align: center; }
                    </style>
                </head>
                <body>
                    <div class='login-container'>
                        <h2>Hangfire Admin</h2>
                        <div class='error'>Invalid login attempt or insufficient permissions.</div>
                        <form method='post' action='/hangfire-login'>
                            <input type='hidden' name='returnUrl' value='" + (returnUrl ?? "/hangfire") + @"' />
                            <div class='form-group'>
                                <label for='email'>Email</label>
                                <input type='text' id='email' name='email' required />
                            </div>
                            <div class='form-group'>
                                <label for='password'>Password</label>
                                <input type='password' id='password' name='password' required />
                            </div>
                            <button type='submit'>Login</button>
                        </form>
                    </div>
                </body>
                </html>";

            return Content(errorHtml, "text/html");
        }
    }
}
