using AuthMiddlware.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AuthMiddlware.Controllers
{
    public class SignInController : Controller
    {
        private readonly IConfiguration _configuration;

        public SignInController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View(new UserModel());
        }

        [HttpPost]
        public async Task<IActionResult> IndexAsync(UserModel user)
        {
            string password = user.Password.ToHashPassword(user.Email, "123");
            if (user.Email == "slh@gmail.com" &&
                password == "e2632eb61f4d9e6e8c223429bdf6ec4aa14cd67c3fb16c5a50ed413f95973d67")
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("session_expires", user.RememberMe ?  
                        DateTime.Now.AddDays(7).ToString("o") :
                        DateTime.Now.AddMinutes(30).ToString("o"))
                };

                var identity = new ClaimsIdentity(claims, "login");
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(principal);

                return RedirectToAction("Index", "Home");
            }

            return View(user);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index", "SignIn");
        }
    }

    public static class DevCode
    {
        public static string ToHashPassword(this string password, string userName, string secretKey)
        {
            // Create a SHA256   
            using SHA256 sha256Hash = SHA256.Create();
            // ComputeHash - returns byte array  
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(
                password +
                userName
                .Replace("a", "@")
                .Replace("l", "!") +
                secretKey));

            // Convert byte array to a string   
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }
    }
}
