using Domain;

namespace Application.Documents.Dtos;

public sealed record ContractListItemDto(
    Guid Id,
    Guid ClientId,
    string Title,
    ContractStatus Status,
    DateTime? SignedAtUtc,
    DateTime? ExpiresAtUtc,
    DateTime UpdatedAtUtc);
