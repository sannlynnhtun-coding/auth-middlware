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
            return Ok("Cookie protected OK");
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult SetupCookie()
        {
            var options = new CookieOptions { Expires = DateTime.Now.AddMinutes(1) };
            Response.Cookies.Append("email", "cookie-test@example.com", options);
            return Ok("Cookie setup complete");
        }
    }
}
