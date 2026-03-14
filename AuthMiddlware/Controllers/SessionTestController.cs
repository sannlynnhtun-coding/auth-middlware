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
            return Ok("Session protected OK");
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult SetupSession()
        {
            HttpContext.Session.SetString("email", "session-test@example.com");
            return Ok("Session setup complete");
        }
    }
}
