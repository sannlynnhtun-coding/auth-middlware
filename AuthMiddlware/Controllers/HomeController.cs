using AuthMiddlware.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;

namespace AuthMiddlware.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var model = new HomeDashboardViewModel
            {
                SessionEmail = HttpContext.Session.GetString("email"),
                CookieEmail = HttpContext.Request.Cookies["email"],
                JwtTokenRaw = HttpContext.Request.Cookies["jwt_token"]
            };

            if (!string.IsNullOrWhiteSpace(model.JwtTokenRaw))
            {
                model.JwtTokenMasked = MaskToken(model.JwtTokenRaw);

                try
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var jwt = tokenHandler.ReadJwtToken(model.JwtTokenRaw);
                    model.Claims = jwt.Claims
                        .Select(x => new HomeDashboardClaimItem { Type = x.Type, Value = x.Value })
                        .ToList();

                    model.JwtDecodeSuccess = true;
                    model.JwtDecodeMessage = model.Claims.Count == 0
                        ? "Token is valid but no claims were found."
                        : "Decoded claims from jwt_token cookie.";
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to decode JWT token from cookie.");
                    model.JwtDecodeSuccess = false;
                    model.JwtDecodeMessage = "Token exists, but decoding failed. Please sign in again.";
                }
            }

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private static string MaskToken(string token)
        {
            if (token.Length <= 34)
            {
                return token;
            }

            return $"{token[..18]}...{token[^12..]}";
        }
    }
}
