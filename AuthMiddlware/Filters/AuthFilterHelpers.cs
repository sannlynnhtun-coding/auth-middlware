using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AuthMiddlware.Filters
{
    internal static class AuthFilterHelpers
    {
        public static bool IsAnonymousEndpoint(ActionExecutingContext context)
        {
            if (context.Filters.Any(x => x is IAllowAnonymousFilter))
            {
                return true;
            }

            return context.HttpContext.GetEndpoint()?.Metadata.GetMetadata<IAllowAnonymous>() is not null;
        }

        public static void RedirectToSignIn(ActionExecutingContext context)
        {
            context.Result = new RedirectToActionResult("Index", "SignIn", null);
        }
    }
}
