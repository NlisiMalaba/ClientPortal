namespace Infrastructure.Auth;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public string PrivateKeyPem { get; set; } = string.Empty;

    public string PublicKeyPem { get; set; } = string.Empty;

    public int AccessTokenLifetimeMinutes { get; set; } = 15;
}
