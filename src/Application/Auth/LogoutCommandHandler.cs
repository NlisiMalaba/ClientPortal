using Application.Abstractions;
using Application.Auth.Abstractions;
using Domain;
using MediatR;
using Shared;

namespace Application.Auth;

public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result>
{
    private readonly IUserAuthenticationRepository _userAuthenticationRepository;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IRefreshTokenCookieStore _refreshTokenCookieStore;
    private readonly IUnitOfWork _unitOfWork;

    public LogoutCommandHandler(
        IUserAuthenticationRepository userAuthenticationRepository,
        IRefreshTokenService refreshTokenService,
        IRefreshTokenCookieStore refreshTokenCookieStore,
        IUnitOfWork unitOfWork)
    {
        _userAuthenticationRepository = userAuthenticationRepository;
        _refreshTokenService = refreshTokenService;
        _refreshTokenCookieStore = refreshTokenCookieStore;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        string refreshTokenHash = _refreshTokenService.Hash(request.RefreshToken);
        User? user = await _userAuthenticationRepository.FindByRefreshTokenHashAsync(refreshTokenHash, cancellationToken);

        if (user is not null)
        {
            user.RevokeRefreshToken(refreshTokenHash, DateTime.UtcNow);
            _userAuthenticationRepository.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        await _refreshTokenCookieStore.ClearAsync(cancellationToken);
        return Result.Success();
    }
}
