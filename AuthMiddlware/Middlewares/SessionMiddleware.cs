namespace AuthMiddlware.Middlewares
{
    public class SessionMiddleware
    {
        private readonly RequestDelegate _next;

        public SessionMiddleware(RequestDelegate next)
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
            string url = context.Request.Path;
            if (passUrlList.Count(x => x.ToLower() == url.ToLower()) > 0 || url.ToLower() == signInUrl.ToLower())
                goto Result;

            #region Check Session

            if (string.IsNullOrWhiteSpace(context.Session.GetString("email")))
            {
                context.Response.Redirect(signInUrl);
            }

            var email = context.Session.GetString("email");
            if (string.IsNullOrWhiteSpace(email))
            {
                context.Response.Redirect(signInUrl);
            }

            #endregion

            Result:
            // Call the next delegate/middleware in the pipeline.
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
}