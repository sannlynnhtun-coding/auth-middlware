using AuthMiddlware.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthMiddlware.Middlewares
{
    // Legacy middleware retained for reference. Runtime strategy now uses JwtAuthAttribute/JwtCookieAuthFilter (IAsyncActionFilter).
    public class JwtTokenMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly HashSet<string> _bypassPaths;

        public JwtTokenMiddleware(RequestDelegate next, IConfiguration configuration, IOptions<AuthStrategyOptions> authOptions)
        {
            _next = next;
            _configuration = configuration;
            _bypassPaths = BuildBypassPathSet(authOptions.Value.BypassPaths);
        }

        private const string SignInUrl = "/SignIn/Index";

        public async Task InvokeAsync(HttpContext context)
        {
            var url = context.Request.Path.Value ?? string.Empty;
            if (IsBypassed(url))
            {
                await _next(context);
                return;
            }

            var jwtToken = context.Request.Cookies["jwt_token"];
            if (string.IsNullOrWhiteSpace(jwtToken))
            {
                context.Response.Redirect(SignInUrl);
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
                context.Response.Redirect(SignInUrl);
                return;
            }

            var sessionClaim = decodedToken.Claims.FirstOrDefault(x => x.Type == "SessionExpired");
            var canParseExpiry = DateTime.TryParse(sessionClaim?.Value, out var tokenSessionExpired);
            if (!canParseExpiry || DateTime.Now > tokenSessionExpired)
            {
                context.Response.Redirect(SignInUrl);
                return;
            }

            var emailClaim = decodedToken.Claims.FirstOrDefault(x => x.Type == "email");
            if (emailClaim is null)
            {
                context.Response.Redirect(SignInUrl);
                return;
            }

            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];
            var keyValue = _configuration["Jwt:Key"];
            if (string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience) || string.IsNullOrWhiteSpace(keyValue))
            {
                context.Response.Redirect(SignInUrl);
                return;
            }

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
            var refreshedJwt = tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));

            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.Now.AddMinutes(1)
            };
            context.Response.Cookies.Append("jwt_token", refreshedJwt, cookieOptions);

            await _next(context);
        }

        private bool IsBypassed(string url)
        {
            return _bypassPaths.Contains(url);
        }

        private static HashSet<string> BuildBypassPathSet(IEnumerable<string> bypassPaths)
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                SignInUrl
            };

            foreach (var path in bypassPaths)
            {
                if (!string.IsNullOrWhiteSpace(path))
                {
                    set.Add(path.Trim());
                }
            }

            return set;
        }
    }

    public static class JwtTokenMiddlewareExtension
    {
        public static IApplicationBuilder UseJwtMiddleware(this IApplicationBuilder app)
        {
            return app.UseMiddleware<JwtTokenMiddleware>();
        }
    }
}
