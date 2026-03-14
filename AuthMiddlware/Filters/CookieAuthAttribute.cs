using Microsoft.AspNetCore.Mvc;

namespace AuthMiddlware.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class CookieAuthAttribute : TypeFilterAttribute
    {
        public CookieAuthAttribute() : base(typeof(CookieAuthFilter))
        {
        }
    }
}
