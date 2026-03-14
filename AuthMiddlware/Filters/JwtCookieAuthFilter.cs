using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthMiddlware.Filters
{
    internal sealed class JwtCookieAuthFilter : IAsyncActionFilter
    {
        private readonly IConfiguration _configuration;

        public JwtCookieAuthFilter(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (AuthFilterHelpers.IsAnonymousEndpoint(context))
            {
                await next();
                return;
            }

            var jwtToken = context.HttpContext.Request.Cookies["jwt_token"];
            if (string.IsNullOrWhiteSpace(jwtToken))
            {
                AuthFilterHelpers.SetJsonAuthFailure(context, "jwt", "JWT token not found. Please run demo sign-in first.");
                return;
            }

            JwtSecurityToken decodedToken;
            try
            {
                var handler = new JwtSecurityTokenHandler();
                decodedToken = handler.ReadJwtToken(jwtToken);
            }
            catch
            {
                AuthFilterHelpers.SetJsonAuthFailure(context, "jwt", "JWT token is invalid. Please sign in again.");
                return;
            }

            var sessionExpiredClaim = decodedToken.Claims.FirstOrDefault(x => x.Type == "SessionExpired");
            var canParseExpiry = DateTime.TryParse(sessionExpiredClaim?.Value, out var tokenSessionExpired);
            if (!canParseExpiry || DateTime.Now > tokenSessionExpired)
            {
                AuthFilterHelpers.SetJsonAuthFailure(context, "jwt", "JWT token expired. Please run demo sign-in again.");
                return;
            }

            var emailClaim = decodedToken.Claims.FirstOrDefault(x => x.Type == "email");
            if (emailClaim is null)
            {
                AuthFilterHelpers.SetJsonAuthFailure(context, "jwt", "JWT email claim is missing.");
                return;
            }

            var keyValue = _configuration["Jwt:Key"];
            if (string.IsNullOrWhiteSpace(keyValue))
            {
                AuthFilterHelpers.SetJsonAuthFailure(context, "jwt", "JWT configuration is missing.");
                return;
            }

            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];
            var key = Encoding.ASCII.GetBytes(keyValue);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("Id", Guid.NewGuid().ToString()),
                    new Claim("SessionExpired", DateTime.UtcNow.AddSeconds(5).ToString("o")),
                    new Claim(JwtRegisteredClaimNames.Email, emailClaim.Value),
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
            var refreshedToken = tokenHandler.CreateToken(tokenDescriptor);
            var refreshedJwt = tokenHandler.WriteToken(refreshedToken);

            var options = new CookieOptions
            {
                Expires = DateTime.Now.AddMinutes(1)
            };

            context.HttpContext.Response.Cookies.Append("jwt_token", refreshedJwt, options);

            await next();
        }
    }
}
