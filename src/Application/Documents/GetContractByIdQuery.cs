using Application.Documents.Dtos;
using MediatR;
using Shared;

namespace Application.Documents;

public sealed record GetContractByIdQuery(Guid ContractId) : IRequest<Result<ContractDto>>;
