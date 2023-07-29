using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthMiddlware.Middlewares
{
    public class JwtTokenMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;

        public JwtTokenMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        string signInUrl = "/SignIn/Index";
        List<string> passUrlList = new List<string>
        {
            "/SignIn/Index",
        };
        public async Task InvokeAsync(HttpContext context)
        {
            string url = context.Request.Path;
            if (passUrlList.Count(x =>
                x.ToLower() == url.ToLower()) > 0 ||
                url.ToLower() == signInUrl.ToLower())
                goto Result;

            var jwtToken = context.Request.Cookies["jwt_token"];
            if (string.IsNullOrWhiteSpace(jwtToken))
            {
                context.Response.Redirect(signInUrl);
                goto Result;
            }

            var handler = new JwtSecurityTokenHandler();
            var decodedToken = handler.ReadJwtToken(jwtToken);
            //foreach (var claim in decodedToken.Claims)
            //{
            //    Console.WriteLine($"Claim Type: {claim.Type}, Value: {claim.Value}");
            //}

            var item = decodedToken.Claims.FirstOrDefault(x => x.Type == "SessionExpired");
            DateTime tokenSessionExpired = Convert.ToDateTime(item?.Value);
            if (item is null || DateTime.Now > tokenSessionExpired)
            {
                context.Response.Redirect(signInUrl);
                goto Result;
            }

            //DateTime tokenSessionExpired = Convert.ToDateTime(item?.Value).ToUniversalTime();
            //if (item is null || DateTime.UtcNow > tokenSessionExpired)
            //{
            //    context.Response.Redirect(signInUrl);
            //    goto Result;
            //}

            var itemEmail = decodedToken.Claims.FirstOrDefault(x => x.Type == "email");
            if (itemEmail is null)
            {
                context.Response.Redirect(signInUrl);
                goto Result;
            }

            if (_configuration == null) goto Result;

            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                        new Claim("Id", Guid.NewGuid().ToString()),
                        new Claim("SessionExpired", DateTime.UtcNow.AddSeconds(5).ToString("o")),
                        new Claim(JwtRegisteredClaimNames.Email, itemEmail.Value),
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
            jwtToken = tokenHandler.WriteToken(token);

            CookieOptions options = new CookieOptions();
            options.Expires = DateTime.Now.AddMinutes(1);

            context.Response.Cookies.Append("jwt_token", jwtToken, options);

        Result:
            // Call the next delegate/middleware in the pipeline.
            await _next(context);
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
