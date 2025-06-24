using System.Security.Claims;

namespace AuthMiddlware.Middlewares;

public class SessionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly List<string> passUrlList = new() { "/SignIn/Index" };

    public SessionMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        string url = context.Request.Path;

        if (passUrlList.Any(x => x.Equals(url, StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context);
            return;
        }

        //if (!context.User.Identity!.IsAuthenticated)
        //{
        //    context.Response.Redirect("/SignIn/Index");
        //    return;
        //}

        var expiresStr = context.User.FindFirstValue("session_expires");
        if (!DateTimeOffset.TryParse(expiresStr, out var expires) || expires < DateTime.Now)
        {
            context.Response.Redirect("/SignIn/Index");
            return;
        }

        await _next(context);
    }
}

public static class SessionMiddlewareExtension
{
    public static IApplicationBuilder UseSessionMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<SessionMiddleware>();
    }
}