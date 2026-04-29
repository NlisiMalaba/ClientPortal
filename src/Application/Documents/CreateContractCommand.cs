using Application.Documents.Dtos;
using MediatR;
using Shared;

namespace Application.Documents;

public sealed record CreateContractCommand(
    Guid ClientId,
    string Title,
    string S3Key,
    IReadOnlyCollection<string>? Parties = null,
    DateTime? ExpiresAtUtc = null) : IRequest<Result<ContractDto>>;
