using Application.Abstractions;
using Application.Auth.Abstractions;
using Domain;
using MediatR;
using Shared;

namespace Application.Auth;

public sealed class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result>
{
    private static readonly Error InvalidResetTokenError = new(
        "Auth.InvalidResetToken",
        "Password reset token is invalid or expired.",
        ErrorType.Forbidden);

    private readonly IUserAuthenticationRepository _userAuthenticationRepository;
    private readonly IPasswordResetTokenService _passwordResetTokenService;
    private readonly IPasswordResetTokenStore _passwordResetTokenStore;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public ResetPasswordCommandHandler(
        IUserAuthenticationRepository userAuthenticationRepository,
        IPasswordResetTokenService passwordResetTokenService,
        IPasswordResetTokenStore passwordResetTokenStore,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork)
    {
        _userAuthenticationRepository = userAuthenticationRepository;
        _passwordResetTokenService = passwordResetTokenService;
        _passwordResetTokenStore = passwordResetTokenStore;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        string tokenHash = _passwordResetTokenService.Hash(request.Token);
        PasswordResetTokenRecord? tokenRecord = await _passwordResetTokenStore.FindValidByHashAsync(tokenHash, cancellationToken);
        if (tokenRecord is null || tokenRecord.ExpiresAtUtc <= DateTime.UtcNow)
        {
            return Result.Failure(InvalidResetTokenError);
        }

        User? user = await _userAuthenticationRepository.FindByIdAsync(tokenRecord.UserId, cancellationToken);
        if (user is null || !user.IsActive)
        {
            return Result.Failure(InvalidResetTokenError);
        }

        user.UpdatePasswordHash(_passwordHasher.Hash(request.NewPassword));
        _userAuthenticationRepository.Update(user);

        await _passwordResetTokenStore.MarkUsedAsync(user.Id, tokenRecord.TokenHash, DateTime.UtcNow, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
