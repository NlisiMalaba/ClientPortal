using MediatR;
using Shared;

namespace Application.Documents;

public sealed record DeleteContractCommand(
    Guid ContractId,
    Guid ClientId) : IRequest<Result>;
