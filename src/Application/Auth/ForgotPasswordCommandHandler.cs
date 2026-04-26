using Application.Auth.Abstractions;
using Domain;
using MediatR;
using Shared;

namespace Application.Auth;

public sealed class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result>
{
    private readonly IUserAuthenticationRepository _userAuthenticationRepository;
    private readonly IPasswordResetTokenService _passwordResetTokenService;
    private readonly IPasswordResetTokenStore _passwordResetTokenStore;
    private readonly IPasswordResetNotificationService _passwordResetNotificationService;

    public ForgotPasswordCommandHandler(
        IUserAuthenticationRepository userAuthenticationRepository,
        IPasswordResetTokenService passwordResetTokenService,
        IPasswordResetTokenStore passwordResetTokenStore,
        IPasswordResetNotificationService passwordResetNotificationService)
    {
        _userAuthenticationRepository = userAuthenticationRepository;
        _passwordResetTokenService = passwordResetTokenService;
        _passwordResetTokenStore = passwordResetTokenStore;
        _passwordResetNotificationService = passwordResetNotificationService;
    }

    public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        EmailAddress email = new(request.Email);
        User? user = await _userAuthenticationRepository.FindByEmailAsync(email, cancellationToken);

        // Always return success to prevent account enumeration.
        if (user is null || !user.IsActive)
        {
            return Result.Success();
        }

        PasswordResetTokenIssueResult token = _passwordResetTokenService.Issue();
        await _passwordResetTokenStore.StoreAsync(user.Id, token.TokenHash, token.ExpiresAtUtc, cancellationToken);
        await _passwordResetNotificationService.SendResetPasswordAsync(
            user,
            token.Token,
            token.ExpiresAtUtc,
            cancellationToken);

        return Result.Success();
    }
}
