using AuthMiddlware.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthMiddlware.Controllers
{
    [CookieAuth]
    public class CookieTestController : Controller
    {
        [HttpGet]
        public IActionResult Protected()
        {
            return Ok(new
            {
                success = true,
                filter = "cookie",
                message = "Cookie filter check passed.",
                timestampUtc = DateTime.UtcNow
            });
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult SetupCookie()
        {
            var options = new CookieOptions { Expires = DateTime.Now.AddMinutes(1) };
            Response.Cookies.Append("email", "cookie-test@example.com", options);

            return Ok(new
            {
                success = true,
                filter = "cookie",
                message = "Cookie setup complete.",
                timestampUtc = DateTime.UtcNow
            });
        }
    }
}
