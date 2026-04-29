using Application.Clients.Dtos;
using MediatR;
using Shared;

namespace Application.Clients;

public sealed record GetClientByIdQuery(Guid ClientId) : IRequest<Result<ClientDetailDto>>;
