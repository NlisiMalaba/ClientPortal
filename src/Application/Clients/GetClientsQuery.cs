using Application.Clients.Dtos;
using Domain;
using MediatR;
using Shared;

namespace Application.Clients;

public sealed record GetClientsQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    ClientStatus? Status = null) : IRequest<Result<PagedResult<ClientListItemDto>>>;
