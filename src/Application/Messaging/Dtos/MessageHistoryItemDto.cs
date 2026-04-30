using Domain;

namespace Application.Messaging.Dtos;

public sealed record MessageHistoryItemDto(
    Guid Id,
    Guid ThreadId,
    Guid SenderId,
    string SenderRole,
    string ClientMessageId,
    long SequenceNumber,
    string Content,
    MessageStatus Status,
    DateTime SentAt,
    DateTime? DeliveredAt,
    DateTime? ReadAt);
