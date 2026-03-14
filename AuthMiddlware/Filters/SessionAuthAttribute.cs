using Microsoft.AspNetCore.Mvc;

namespace AuthMiddlware.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class SessionAuthAttribute : TypeFilterAttribute
    {
        public SessionAuthAttribute() : base(typeof(SessionAuthFilter))
        {
        }
    }
}
