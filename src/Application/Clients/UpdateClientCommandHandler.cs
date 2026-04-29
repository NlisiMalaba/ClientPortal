using Application.Abstractions;
using Application.Clients.Abstractions;
using Domain;
using MediatR;
using Shared;

namespace Application.Clients;

public sealed class UpdateClientCommandHandler : IRequestHandler<UpdateClientCommand, Result>
{
    private static readonly Error ClientNotFoundError = new(
        "Clients.NotFound",
        "Client was not found.",
        ErrorType.NotFound);

    private static readonly Error InvalidStatusTransitionError = new(
        "Clients.InvalidStatusTransition",
        "The requested client status transition is not supported.",
        ErrorType.Validation);

    private readonly IClientRepository _clientRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateClientCommandHandler(
        IClientRepository clientRepository,
        IUnitOfWork unitOfWork)
    {
        _clientRepository = clientRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateClientCommand request, CancellationToken cancellationToken)
    {
        Client? client = await _clientRepository.FindByIdAsync(request.ClientId, cancellationToken);
        if (client is null)
        {
            return Result.Failure(ClientNotFoundError);
        }

        client.UpdateProfile(
            request.CompanyName,
            request.ContactName,
            new EmailAddress(request.Email),
            new PhoneNumber(request.Phone));
        client.UpdateNotes(request.Notes);

        Result statusResult = ApplyStatus(client, request.Status);
        if (statusResult.IsFailed)
        {
            return statusResult;
        }

        _clientRepository.Update(client);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private static Result ApplyStatus(Client client, ClientStatus requestedStatus)
    {
        switch (requestedStatus)
        {
            case ClientStatus.Invited:
                if (client.Status != ClientStatus.Invited)
                {
                    return Result.Failure(InvalidStatusTransitionError);
                }

                return Result.Success();

            case ClientStatus.Active:
                if (client.Status == ClientStatus.Invited)
                {
                    client.Onboard(DateTime.UtcNow);
                }
                else
                {
                    client.Activate();
                }

                return Result.Success();

            case ClientStatus.Inactive:
                client.Deactivate(DateTime.UtcNow);
                return Result.Success();

            case ClientStatus.Suspended:
                client.Suspend();
                return Result.Success();

            default:
                return Result.Failure(InvalidStatusTransitionError);
        }
    }
}
