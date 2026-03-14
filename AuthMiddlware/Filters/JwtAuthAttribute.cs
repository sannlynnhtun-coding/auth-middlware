using Microsoft.AspNetCore.Mvc;

namespace AuthMiddlware.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class JwtAuthAttribute : TypeFilterAttribute
    {
        public JwtAuthAttribute() : base(typeof(JwtCookieAuthFilter))
        {
        }
    }
}
