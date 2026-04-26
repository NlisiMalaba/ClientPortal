using Application.Auth.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Api.Auth;

public sealed class HttpContextRefreshTokenCookieStore : IRefreshTokenCookieStore
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly RefreshTokenCookieOptions _options;

    public HttpContextRefreshTokenCookieStore(
        IHttpContextAccessor httpContextAccessor,
        IOptions<RefreshTokenCookieOptions> options)
    {
        _httpContextAccessor = httpContextAccessor;
        _options = options.Value;
        ValidateOptions(_options);
    }

    public Task SetAsync(string refreshToken, DateTime expiresAt, CancellationToken cancellationToken = default)
    {
        HttpContext httpContext = GetHttpContext();
        DateTime cookieExpiry = DateTime.UtcNow.AddDays(_options.ExpiryDays);
        CookieOptions cookieOptions = CreateCookieOptions(cookieExpiry);
        httpContext.Response.Cookies.Append(_options.Name, refreshToken, cookieOptions);
        return Task.CompletedTask;
    }

    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        HttpContext httpContext = GetHttpContext();
        CookieOptions cookieOptions = CreateCookieOptions(DateTime.UtcNow.AddDays(-1));
        httpContext.Response.Cookies.Delete(_options.Name, cookieOptions);
        return Task.CompletedTask;
    }

    private CookieOptions CreateCookieOptions(DateTime expiresAtUtc)
    {
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = expiresAtUtc
        };
    }

    private HttpContext GetHttpContext()
    {
        return _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("No active HTTP context is available.");
    }

    private static void ValidateOptions(RefreshTokenCookieOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Name))
        {
            throw new InvalidOperationException("RefreshTokenCookie:Name must be configured.");
        }

        if (options.ExpiryDays <= 0)
        {
            throw new InvalidOperationException("RefreshTokenCookie:ExpiryDays must be greater than zero.");
        }
    }
}
