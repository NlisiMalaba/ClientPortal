using Application.Clients.Dtos;
using MediatR;
using Shared;

namespace Application.Clients;

public sealed record InviteClientCommand(
    string CompanyName,
    string ContactName,
    string Email,
    string Phone,
    string? Notes = null) : IRequest<Result<InviteClientResultDto>>;
