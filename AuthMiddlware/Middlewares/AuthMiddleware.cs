using System;

namespace AuthMiddlware.Middlewares;

public class AuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly List<string> passUrlList = new() { "/SignIn/Index" };

    public AuthMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        string url = context.Request.Path;

        if (passUrlList.Any(x => x.Equals(url, StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context);
            return;
        }

        if (!context.User.Identity!.IsAuthenticated)
        {
            context.Response.Redirect("/SignIn/Index");
            return;
        }

        await _next(context);
    }
}

public static class AuthMiddlewareExtension
{
    public static IApplicationBuilder UseAuthMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<AuthMiddleware>();
    }
}