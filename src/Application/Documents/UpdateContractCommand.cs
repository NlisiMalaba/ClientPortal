using MediatR;
using Shared;

namespace Application.Documents;

public sealed record UpdateContractCommand(
    Guid ContractId,
    Guid ClientId,
    string Title,
    string S3Key,
    IReadOnlyCollection<string>? Parties,
    DateTime? ExpiresAtUtc) : IRequest<Result>;
