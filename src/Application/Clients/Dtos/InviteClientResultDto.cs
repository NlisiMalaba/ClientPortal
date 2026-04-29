namespace Application.Clients.Dtos;

public sealed record InviteClientResultDto(
    Guid ClientId,
    Guid ClientUserId);
