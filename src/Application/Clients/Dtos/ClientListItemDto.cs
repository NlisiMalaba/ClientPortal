using Domain;

namespace Application.Clients.Dtos;

public sealed record ClientListItemDto(
    Guid Id,
    string CompanyName,
    string ContactName,
    string Email,
    string Phone,
    ClientStatus Status,
    DateTime InvitedAt,
    DateTime? OnboardedAt);
