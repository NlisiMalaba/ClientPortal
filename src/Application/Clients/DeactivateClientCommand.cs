using MediatR;
using Shared;

namespace Application.Clients;

public sealed record DeactivateClientCommand(Guid ClientId) : IRequest<Result>;
