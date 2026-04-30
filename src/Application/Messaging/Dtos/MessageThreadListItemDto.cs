namespace Application.Messaging.Dtos;

public sealed record MessageThreadListItemDto(
    Guid Id,
    Guid ClientId,
    Guid? ProjectId,
    string Subject,
    DateTime LastMessageAt,
    int UnreadCount);
