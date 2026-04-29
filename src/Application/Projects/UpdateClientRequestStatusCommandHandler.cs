using Application.Abstractions;
using Application.Projects.Abstractions;
using Domain;
using MediatR;
using Shared;

namespace Application.Projects;

public sealed class UpdateClientRequestStatusCommandHandler : IRequestHandler<UpdateClientRequestStatusCommand, Result>
{
    private static readonly Error ClientRequestNotFoundError = new(
        "ClientRequests.NotFound",
        "Client request was not found.",
        ErrorType.NotFound);

    private readonly IClientRequestRepository _clientRequestRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateClientRequestStatusCommandHandler(
        IClientRequestRepository clientRequestRepository,
        IUnitOfWork unitOfWork)
    {
        _clientRequestRepository = clientRequestRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateClientRequestStatusCommand request, CancellationToken cancellationToken)
    {
        ClientRequest? clientRequest = await _clientRequestRepository.FindByIdAsync(request.RequestId, cancellationToken);
        if (clientRequest is null)
        {
            return Result.Failure(ClientRequestNotFoundError);
        }

        clientRequest.UpdateStatus(request.Status);

        _clientRequestRepository.Update(clientRequest);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
