namespace AuthMiddlware.Models
{
    public sealed class HomeDashboardViewModel
    {
        public string? SessionEmail { get; set; }
        public string? CookieEmail { get; set; }
        public string? JwtTokenRaw { get; set; }
        public string? JwtTokenMasked { get; set; }
        public bool HasJwtToken => !string.IsNullOrWhiteSpace(JwtTokenRaw);
        public bool JwtDecodeSuccess { get; set; }
        public string JwtDecodeMessage { get; set; } = "Token not found. Sign in first to see claims.";
        public List<HomeDashboardClaimItem> Claims { get; set; } = new();

        public bool HasSessionEmail => !string.IsNullOrWhiteSpace(SessionEmail);
        public bool HasCookieEmail => !string.IsNullOrWhiteSpace(CookieEmail);
    }

    public sealed class HomeDashboardClaimItem
    {
        public string Type { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}
