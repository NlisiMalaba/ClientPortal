using Domain;

namespace Application.Auth.Dtos;

public sealed record AuthUserDto(
    Guid Id,
    string Email,
    string FullName,
    Role Role,
    bool IsActive);
