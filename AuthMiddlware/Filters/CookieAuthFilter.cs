using Microsoft.AspNetCore.Mvc.Filters;

namespace AuthMiddlware.Filters
{
    internal sealed class CookieAuthFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (AuthFilterHelpers.IsAnonymousEndpoint(context))
            {
                await next();
                return;
            }

            var email = context.HttpContext.Request.Cookies["email"];
            if (string.IsNullOrWhiteSpace(email))
            {
                AuthFilterHelpers.RedirectToSignIn(context);
                return;
            }

            await next();
        }
    }
}
