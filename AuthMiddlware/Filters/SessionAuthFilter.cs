using Microsoft.AspNetCore.Mvc.Filters;

namespace AuthMiddlware.Filters
{
    internal sealed class SessionAuthFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (AuthFilterHelpers.IsAnonymousEndpoint(context))
            {
                await next();
                return;
            }

            var email = context.HttpContext.Session.GetString("email");
            if (string.IsNullOrWhiteSpace(email))
            {
                AuthFilterHelpers.SetJsonAuthFailure(context, "session", "Session check failed. Please run demo sign-in first.");
                return;
            }

            await next();
        }
    }
}
