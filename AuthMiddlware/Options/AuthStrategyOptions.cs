namespace AuthMiddlware.Options
{
    public sealed class AuthStrategyOptions
    {
        public const string SectionName = "AuthStrategy";

        public string Mode { get; set; } = AuthStrategyMode.Filter;

        public List<string> BypassPaths { get; set; } = new();
    }

    public static class AuthStrategyMode
    {
        public const string Filter = "Filter";
        public const string CookieFilter = "CookieFilter";
        public const string SessionFilter = "SessionFilter";
        public const string JwtMiddleware = "JwtMiddleware";

        public static string Normalize(string? mode)
        {
            return mode?.Trim() switch
            {
                CookieFilter => CookieFilter,
                SessionFilter => SessionFilter,
                JwtMiddleware => JwtMiddleware,
                _ => Filter
            };
        }
    }
}
