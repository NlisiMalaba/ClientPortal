using Application.Documents.Dtos;
using Domain;
using MediatR;
using Shared;

namespace Application.Documents;

public sealed record GetContractsQuery(
    int Page = 1,
    int PageSize = 20,
    Guid? ClientId = null,
    ContractStatus? Status = null) : IRequest<Result<PagedResult<ContractListItemDto>>>;
