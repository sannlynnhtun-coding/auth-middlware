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

        public static void SetJsonAuthFailure(ActionExecutingContext context, string filter, string message)
        {
            context.Result = new JsonResult(new
            {
                success = false,
                filter,
                message,
                statusCode = StatusCodes.Status401Unauthorized,
                timestampUtc = DateTime.UtcNow
            })
            {
                StatusCode = StatusCodes.Status401Unauthorized
            };
        }
    }
}
