using AuthMiddlware.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AuthMiddlware.Controllers
{
    [AllowAnonymous]
    public class SignInController : Controller
    {
        private readonly IConfiguration _configuration;

        public SignInController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View(new UserModel
            {
                Email = "slh@gmail.com",
                Password = "123"
            });
        }

        [HttpPost]
        public IActionResult Index(UserModel user)
        {
            string password = user.Password.ToHashPassword(user.Email, "123");
            if (user.Email == "slh@gmail.com"
                && password == "e2632eb61f4d9e6e8c223429bdf6ec4aa14cd67c3fb16c5a50ed413f95973d67"
                )
            {
                HttpContext.Session.SetString("email", user.Email);

                CookieOptions options = new CookieOptions();
                options.Expires = DateTime.Now.AddMinutes(1);
                HttpContext.Response.Cookies.Append("email", user.Email, options);

                var issuer = _configuration["Jwt:Issuer"];
                var audience = _configuration["Jwt:Audience"];
                var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim("Id", Guid.NewGuid().ToString()),
                        new Claim("SessionExpired", DateTime.Now.AddMinutes(15).ToString("o")),
                        new Claim(JwtRegisteredClaimNames.Email, user.Email),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                    }),
                    Expires = DateTime.UtcNow.AddMinutes(10),
                    Issuer = issuer,
                    Audience = audience,
                    SigningCredentials = new SigningCredentials
                    (new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha512Signature)
                };
                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var jwtToken = tokenHandler.WriteToken(token);
                HttpContext.Response.Headers.Append("jwt_token", jwtToken);
                HttpContext.Response.Cookies.Append("jwt_token", jwtToken, options);
                HttpContext.Session.SetString("jwt_token", jwtToken);

                return RedirectToAction("Index", "Home");
            }

            ViewData["ErrorMessage"] = "Demo login failed. Use the prefilled credentials and click Run Demo.";
            return View(user);
        }
    }

    public static class DevCode
    {
        public static string ToHashPassword(this string password, string userName, string secretKey)
        {
            using SHA256 sha256Hash = SHA256.Create();
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(
                password +
                userName
                .Replace("a", "@")
                .Replace("l", "!") +
                secretKey));

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }
    }
}
