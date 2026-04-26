using Application.Auth.Abstractions;
using Application.Auth.Dtos;
using Domain;
using MediatR;
using Shared;

namespace Application.Auth;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthTokenDto>>
{
    private static readonly Error InvalidCredentialsError = new(
        "Auth.InvalidCredentials",
        "Invalid email or password.",
        ErrorType.Forbidden);

    private static readonly Error UserInactiveError = new(
        "Auth.UserInactive",
        "User account is inactive.",
        ErrorType.Forbidden);

    private readonly IUserAuthenticationRepository _userAuthenticationRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAccessTokenIssuer _accessTokenIssuer;

    public LoginCommandHandler(
        IUserAuthenticationRepository userAuthenticationRepository,
        IPasswordHasher passwordHasher,
        IAccessTokenIssuer accessTokenIssuer)
    {
        _userAuthenticationRepository = userAuthenticationRepository;
        _passwordHasher = passwordHasher;
        _accessTokenIssuer = accessTokenIssuer;
    }

    public async Task<Result<AuthTokenDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        EmailAddress email = new(request.Email);
        User? user = await _userAuthenticationRepository.FindByEmailAsync(email, cancellationToken);
        if (user is null)
        {
            return Result<AuthTokenDto>.Failure(InvalidCredentialsError);
        }

        if (!user.IsActive)
        {
            return Result<AuthTokenDto>.Failure(UserInactiveError);
        }

        bool passwordMatches = _passwordHasher.Verify(request.Password, user.PasswordHash);
        if (!passwordMatches)
        {
            return Result<AuthTokenDto>.Failure(InvalidCredentialsError);
        }

        AccessTokenIssueResult token = _accessTokenIssuer.Issue(user);
        AuthTokenDto authToken = new(
            AccessToken: token.AccessToken,
            ExpiresAt: token.ExpiresAt,
            User: new AuthUserDto(
                Id: user.Id,
                Email: user.Email.Value,
                FullName: user.FullName,
                Role: user.Role,
                IsActive: user.IsActive));

        return Result<AuthTokenDto>.Success(authToken);
    }
}
