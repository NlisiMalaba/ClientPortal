using Domain;
using MediatR;
using Shared;

namespace Application.Projects;

public sealed record UpdateClientRequestStatusCommand(
    Guid RequestId,
    ClientRequestStatus Status) : IRequest<Result>;
