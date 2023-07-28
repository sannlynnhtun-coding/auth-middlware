using System;

namespace AuthMiddlware.Middlewares
{
    public class CookieMiddleware
    {
        private readonly RequestDelegate _next;

        public CookieMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        string signInUrl = "/SignIn/Index";
        List<string> passUrlList = new List<string>
        {
            "/SignIn/Index",
        };
        public async Task InvokeAsync(HttpContext context)
        {
            //var password = context.Session.GetString("password");
            string url = context.Request.Path;
            if (passUrlList.Count(x => x.ToLower() == url.ToLower()) > 0 || url.ToLower() == signInUrl.ToLower())
                goto Result;

            var email = context.Request.Cookies["email"];
            if (string.IsNullOrWhiteSpace(email))
            {
                context.Response.Redirect(signInUrl);
            }
        //if (passUrlList.Count(x => x.ToLower() == url.ToLower()) > 0 || url.ToLower() == signInUrl.ToLower())
        //    goto Result;

        //#region Check Session

        //if (string.IsNullOrWhiteSpace(context.Session.GetString("email")))
        //{
        //    context.Response.Redirect(signInUrl);
        //}

        //var email = context.Session.GetString("email");
        //if (string.IsNullOrWhiteSpace(email))
        //{
        //    context.Response.Redirect(signInUrl);
        //}

        //#endregion

        Result:
            // Call the next delegate/middleware in the pipeline.
            await _next(context);
        }
    }

    public static class CookieMiddlewareExtension
    {
        public static IApplicationBuilder UseCookieMiddleware(this IApplicationBuilder app)
        {
            return app.UseMiddleware<CookieMiddleware>();
        }
    }
}