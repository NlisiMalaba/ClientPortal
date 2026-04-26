using Application.Abstractions;
using Application.Auth.Abstractions;
using Application.Auth.Dtos;
using Domain;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared;

namespace Application.Auth;

public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthTokenDto>>
{
    private static readonly Error InvalidRefreshTokenError = new(
        "Auth.InvalidRefreshToken",
        "Refresh token is invalid or expired.",
        ErrorType.Forbidden);

    private static readonly Error UserInactiveError = new(
        "Auth.UserInactive",
        "User account is inactive.",
        ErrorType.Forbidden);

    private readonly IUserAuthenticationRepository _userAuthenticationRepository;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IRefreshTokenCookieStore _refreshTokenCookieStore;
    private readonly IAccessTokenIssuer _accessTokenIssuer;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;

    public RefreshTokenCommandHandler(
        IUserAuthenticationRepository userAuthenticationRepository,
        IRefreshTokenService refreshTokenService,
        IRefreshTokenCookieStore refreshTokenCookieStore,
        IAccessTokenIssuer accessTokenIssuer,
        IUnitOfWork unitOfWork,
        ILogger<RefreshTokenCommandHandler> logger)
    {
        _userAuthenticationRepository = userAuthenticationRepository;
        _refreshTokenService = refreshTokenService;
        _refreshTokenCookieStore = refreshTokenCookieStore;
        _accessTokenIssuer = accessTokenIssuer;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<AuthTokenDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        string refreshTokenHash = _refreshTokenService.Hash(request.RefreshToken);
        User? user = await _userAuthenticationRepository.FindByRefreshTokenHashAsync(refreshTokenHash, cancellationToken);
        if (user is null)
        {
            return Result<AuthTokenDto>.Failure(InvalidRefreshTokenError);
        }

        if (!user.IsActive)
        {
            return Result<AuthTokenDto>.Failure(UserInactiveError);
        }

        RefreshToken? presentedToken = user.RefreshTokens.SingleOrDefault(token =>
            _refreshTokenService.Validate(request.RefreshToken, token.TokenHash));
        if (presentedToken is null)
        {
            return Result<AuthTokenDto>.Failure(InvalidRefreshTokenError);
        }

        if (presentedToken.IsRevoked)
        {
            int revokedTokenCount = user.RevokeRefreshTokenFamily(DateTime.UtcNow);
            _userAuthenticationRepository.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _refreshTokenCookieStore.ClearAsync(cancellationToken);

            _logger.LogWarning(
                "Refresh token reuse detected. Revoked refresh token family for user {UserId}. PresentedTokenHash={PresentedTokenHash}, RevokedCount={RevokedCount}, ClientIp={ClientIp}",
                user.Id,
                presentedToken.TokenHash,
                revokedTokenCount,
                request.ClientIp);

            return Result<AuthTokenDto>.Failure(InvalidRefreshTokenError);
        }

        if (!presentedToken.IsActive)
        {
            return Result<AuthTokenDto>.Failure(InvalidRefreshTokenError);
        }

        RefreshTokenIssueResult issuedRefreshToken = _refreshTokenService.Generate(request.ClientIp);
        user.RevokeRefreshToken(
            tokenHash: presentedToken.TokenHash,
            revokedAtUtc: DateTime.UtcNow,
            replacedByToken: issuedRefreshToken.TokenHash);
        user.AddRefreshToken(new RefreshToken(
            tokenHash: issuedRefreshToken.TokenHash,
            expiresAt: issuedRefreshToken.ExpiresAt,
            createdByIp: issuedRefreshToken.CreatedByIp));

        _userAuthenticationRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _refreshTokenCookieStore.SetAsync(
            issuedRefreshToken.Token,
            issuedRefreshToken.ExpiresAt,
            cancellationToken);

        AccessTokenIssueResult accessToken = _accessTokenIssuer.Issue(user);
        AuthTokenDto authToken = new(
            AccessToken: accessToken.AccessToken,
            ExpiresAt: accessToken.ExpiresAt,
            User: new AuthUserDto(
                Id: user.Id,
                Email: user.Email.Value,
                FullName: user.FullName,
                Role: user.Role,
                IsActive: user.IsActive));

        return Result<AuthTokenDto>.Success(authToken);
    }
}
