using AuthMiddlware.Models;
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
        public IActionResult Index(UserModel user)
        {
            string password = user.Password.ToHashPassword(user.Email, "123");
            if (user.Email == "slh@gmail.com"
                //&& user.Password == "123"
                && password == "192193cce00ee219a27de7a7fd4ee5d53f4cc93dd2a1120eab528b2c32b08228"
                )
            {
                #region Session

                HttpContext.Session.SetString("email", user.Email); // server session

                #endregion

                #region Cookie

                CookieOptions options = new CookieOptions();
                options.Expires = DateTime.Now.AddMinutes(1);
                HttpContext.Response.Cookies.Append("email", user.Email, options);

                #endregion

                #region Jwt

                var issuer = _configuration["Jwt:Issuer"];
                var audience = _configuration["Jwt:Audience"];
                var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim("Id", Guid.NewGuid().ToString()),
                        //new Claim("SessionExpired", DateTime.Now.AddMinutes(15).ToString("o")),
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
                HttpContext.Session.SetString("jwt_token",jwtToken);
                var jwt = HttpContext.Session.GetString("jwt_token");
                #endregion

                return RedirectToAction("Index", "Home");
            }
            return View(user);
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
