using AuthMiddlware.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthMiddlware.Controllers
{
    [JwtAuth]
    public class JwtTestController : Controller
    {
        private readonly IConfiguration _configuration;

        public JwtTestController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Protected()
        {
            return Ok(new
            {
                success = true,
                filter = "jwt",
                message = "JWT filter check passed.",
                timestampUtc = DateTime.UtcNow
            });
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult SetupJwt()
        {
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];
            var keyValue = _configuration["Jwt:Key"];
            if (string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience) || string.IsNullOrWhiteSpace(keyValue))
            {
                return BadRequest(new
                {
                    success = false,
                    filter = "jwt",
                    message = "JWT config is missing.",
                    timestampUtc = DateTime.UtcNow
                });
            }

            var key = Encoding.ASCII.GetBytes(keyValue);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("Id", Guid.NewGuid().ToString()),
                    new Claim("SessionExpired", DateTime.Now.AddMinutes(15).ToString("o")),
                    new Claim(JwtRegisteredClaimNames.Email, "middleware-test@example.com"),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                }),
                Expires = DateTime.UtcNow.AddMinutes(10),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha512Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));

            var options = new CookieOptions { Expires = DateTime.Now.AddMinutes(1) };
            Response.Cookies.Append("jwt_token", jwtToken, options);

            return Ok(new
            {
                success = true,
                filter = "jwt",
                message = "JWT setup complete.",
                timestampUtc = DateTime.UtcNow
            });
        }
    }
}
