using Application.Abstractions;
using Application.Projects.Abstractions;
using Domain;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared;

namespace Application.Projects;

public sealed class SubmitClientRequestCommandHandler : IRequestHandler<SubmitClientRequestCommand, Result<Guid>>
{
    private static readonly Error ProjectNotFoundError = new(
        "Projects.NotFound",
        "Project was not found.",
        ErrorType.NotFound);

    private static readonly Error ClientRequestNotificationFailedError = new(
        "ClientRequests.NotificationFailed",
        "Client request was submitted but notifying business staff failed.",
        ErrorType.Unexpected);

    private readonly IProjectRepository _projectRepository;
    private readonly IClientRequestRepository _clientRequestRepository;
    private readonly IClientRequestNotificationService _clientRequestNotificationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SubmitClientRequestCommandHandler> _logger;

    public SubmitClientRequestCommandHandler(
        IProjectRepository projectRepository,
        IClientRequestRepository clientRequestRepository,
        IClientRequestNotificationService clientRequestNotificationService,
        IUnitOfWork unitOfWork,
        ILogger<SubmitClientRequestCommandHandler> logger)
    {
        _projectRepository = projectRepository;
        _clientRequestRepository = clientRequestRepository;
        _clientRequestNotificationService = clientRequestNotificationService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(SubmitClientRequestCommand request, CancellationToken cancellationToken)
    {
        Project? project = await _projectRepository.FindByIdAsync(request.ProjectId, cancellationToken);
        if (project is null || project.ClientId != request.ClientId)
        {
            return Result<Guid>.Failure(ProjectNotFoundError);
        }

        ClientRequest clientRequest = ClientRequest.Create(
            id: Guid.CreateVersion7(),
            clientId: request.ClientId,
            projectId: request.ProjectId,
            title: request.Title,
            description: request.Description,
            priority: request.Priority);

        _clientRequestRepository.Add(clientRequest);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            await _clientRequestNotificationService.NotifySubmittedAsync(clientRequest, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to notify business staff for client request {ClientRequestId}.",
                clientRequest.Id);

            return Result<Guid>.Failure(ClientRequestNotificationFailedError);
        }

        return Result<Guid>.Success(clientRequest.Id);
    }
}
