using Domain;
using MediatR;
using Shared;

namespace Application.Clients;

public sealed record UpdateClientCommand(
    Guid ClientId,
    string CompanyName,
    string ContactName,
    string Email,
    string Phone,
    string? Notes,
    ClientStatus Status) : IRequest<Result>;
