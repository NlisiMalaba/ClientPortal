namespace Application.Auth.Dtos;

public sealed record AuthTokenDto(
    string AccessToken,
    DateTime ExpiresAt,
    AuthUserDto User);
