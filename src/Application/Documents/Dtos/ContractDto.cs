using Domain;

namespace Application.Documents.Dtos;

public sealed record ContractDto(
    Guid Id,
    Guid ClientId,
    string Title,
    ContractStatus Status,
    DateTime? SignedAtUtc,
    DateTime? ExpiresAtUtc,
    string S3Key,
    IReadOnlyCollection<string> Parties,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
