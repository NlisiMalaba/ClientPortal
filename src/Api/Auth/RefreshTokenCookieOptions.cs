namespace Api.Auth;

public sealed class RefreshTokenCookieOptions
{
    public const string SectionName = "RefreshTokenCookie";

    public string Name { get; set; } = "refreshToken";

    public int ExpiryDays { get; set; } = 7;
}
