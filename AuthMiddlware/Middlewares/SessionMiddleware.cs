using AuthMiddlware.Options;
using Microsoft.Extensions.Options;

namespace AuthMiddlware.Middlewares
{
    // Legacy middleware retained for reference. Runtime strategy now uses SessionAuthFilter (IAsyncActionFilter).
    public class SessionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly HashSet<string> _bypassPaths;

        public SessionMiddleware(RequestDelegate next, IOptions<AuthStrategyOptions> authOptions)
        {
            _next = next;
            _bypassPaths = BuildBypassPathSet(authOptions.Value.BypassPaths);
        }

        private const string SignInUrl = "/SignIn/Index";

        public async Task InvokeAsync(HttpContext context)
        {
            var url = context.Request.Path.Value ?? string.Empty;
            if (IsBypassed(url))
            {
                await _next(context);
                return;
            }

            var email = context.Session.GetString("email");
            if (string.IsNullOrWhiteSpace(email))
            {
                context.Response.Redirect(SignInUrl);
                return;
            }

            await _next(context);
        }

        private bool IsBypassed(string url)
        {
            return _bypassPaths.Contains(url);
        }

        private static HashSet<string> BuildBypassPathSet(IEnumerable<string> bypassPaths)
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                SignInUrl
            };

            foreach (var path in bypassPaths)
            {
                if (!string.IsNullOrWhiteSpace(path))
                {
                    set.Add(path.Trim());
                }
            }

            return set;
        }
    }

    public static class SessionMiddlewareExtension
    {
        public static IApplicationBuilder UseSessionMiddleware(this IApplicationBuilder app)
        {
            return app.UseMiddleware<SessionMiddleware>();
        }
    }
}
