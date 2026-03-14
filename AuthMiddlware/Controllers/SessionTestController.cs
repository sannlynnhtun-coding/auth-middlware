using AuthMiddlware.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthMiddlware.Controllers
{
    [SessionAuth]
    public class SessionTestController : Controller
    {
        [HttpGet]
        public IActionResult Protected()
        {
            return Ok(new
            {
                success = true,
                filter = "session",
                message = "Session filter check passed.",
                timestampUtc = DateTime.UtcNow
            });
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult SetupSession()
        {
            HttpContext.Session.SetString("email", "session-test@example.com");

            return Ok(new
            {
                success = true,
                filter = "session",
                message = "Session setup complete.",
                timestampUtc = DateTime.UtcNow
            });
        }
    }
}
