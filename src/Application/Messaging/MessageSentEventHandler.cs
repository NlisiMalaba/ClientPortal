using Application.Abstractions;
using Application.Auth.Abstractions;
using Application.Messaging.Abstractions;
using Application.Messaging.Dtos;
using Application.Notifications.Abstractions;
using Domain;
using MediatR;

namespace Application.Messaging;

public sealed class MessageSentEventHandler : INotificationHandler<MessageSentEvent>
{
    private static readonly string[] FallbackChannels = ["email", "whatsapp", "sms"];

    private readonly IMessageThreadRepository _messageThreadRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly ICurrentTenant _currentTenant;
    private readonly IUserAuthenticationRepository _userAuthenticationRepository;
    private readonly INotificationService _notificationService;
    private readonly IUserPresenceService _userPresenceService;
    private readonly IMessageOfflineFallbackNotifier _offlineFallbackNotifier;
    private readonly IRealtimeMessagingService _realtimeMessagingService;

    public MessageSentEventHandler(
        IMessageThreadRepository messageThreadRepository,
        IMessageRepository messageRepository,
        ICurrentTenant currentTenant,
        IUserAuthenticationRepository userAuthenticationRepository,
        INotificationService notificationService,
        IUserPresenceService userPresenceService,
        IMessageOfflineFallbackNotifier offlineFallbackNotifier,
        IRealtimeMessagingService realtimeMessagingService)
    {
        _messageThreadRepository = messageThreadRepository;
        _messageRepository = messageRepository;
        _currentTenant = currentTenant;
        _userAuthenticationRepository = userAuthenticationRepository;
        _notificationService = notificationService;
        _userPresenceService = userPresenceService;
        _offlineFallbackNotifier = offlineFallbackNotifier;
        _realtimeMessagingService = realtimeMessagingService;
    }

    public async Task Handle(MessageSentEvent notification, CancellationToken cancellationToken)
    {
        Message? message = await _messageRepository.FindByIdAsync(notification.MessageId, cancellationToken);
        if (message is null)
        {
            return;
        }

        RealtimeMessagePayload payload = new(
            message.Id,
            message.ThreadId,
            message.SenderId,
            message.SenderRole,
            message.Content,
            message.ReplyToMessageId,
            message.EmojiReaction,
            message.AttachmentFileName is null
                ? null
                : new MessageAttachmentMetadataDto(
                    message.AttachmentFileName,
                    message.AttachmentContentType ?? string.Empty,
                    message.AttachmentSizeBytes ?? 0,
                    message.AttachmentUrl ?? string.Empty),
            message.AttachmentExpiresAt,
            message.IsSoftDeleted,
            message.DeletedAt,
            message.SequenceNumber,
            message.Status,
            message.SentAt);

        await _realtimeMessagingService.BroadcastMessageAsync(payload, cancellationToken);

        await NotifyParticipantsAsync(message, cancellationToken);
    }

    private async Task NotifyParticipantsAsync(Message message, CancellationToken cancellationToken)
    {
        Domain.MessageThread? thread = await _messageThreadRepository.FindByIdAsync(message.ThreadId, cancellationToken);
        if (thread is null)
        {
            return;
        }

        await SendInAppNotificationsAsync(thread, message, cancellationToken);
        await NotifyOfflineParticipantsAsync(thread, message, cancellationToken);
    }

    private async Task SendInAppNotificationsAsync(Domain.MessageThread thread, Message message, CancellationToken cancellationToken)
    {
        Dictionary<string, string> metadata = new(StringComparer.Ordinal)
        {
            ["threadId"] = message.ThreadId.ToString(),
            ["messageId"] = message.Id.ToString(),
            ["senderId"] = message.SenderId.ToString()
        };

        string body = BuildMessagePreviewBody(message.Content);

        foreach (Guid participantId in thread.Participants.Where(id => id != message.SenderId))
        {
            await _notificationService.SendAsync(
                new NotificationMessage(
                    NotificationChannel.InApp,
                    participantId.ToString(),
                    "New message",
                    body,
                    metadata),
                cancellationToken);
        }
    }

    private async Task NotifyOfflineParticipantsAsync(
        Domain.MessageThread thread,
        Message message,
        CancellationToken cancellationToken)
    {
        int offlineThresholdSeconds = Math.Max(0, _currentTenant.Settings?.OfflineFallbackThresholdSeconds ?? 0);
        if (offlineThresholdSeconds == 0)
        {
            return;
        }

        string[] enabledChannels = (_currentTenant.Settings?.NotificationChannels ?? [])
            .Where(channel => FallbackChannels.Contains(channel, StringComparer.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (enabledChannels.Length == 0)
        {
            return;
        }

        foreach (Guid participantId in thread.Participants.Where(id => id != message.SenderId))
        {
            if (_userPresenceService.IsOnline(participantId))
            {
                continue;
            }

            DateTime? lastSeen = _userPresenceService.GetLastSeenUtc(participantId);
            if (!lastSeen.HasValue || DateTime.UtcNow - lastSeen.Value < TimeSpan.FromSeconds(offlineThresholdSeconds))
            {
                continue;
            }

            if (enabledChannels.Contains("email", StringComparer.OrdinalIgnoreCase))
            {
                await TrySendOfflineEmailAsync(participantId, message, cancellationToken);
            }

            await _offlineFallbackNotifier.NotifyRecipientAsync(
                participantId,
                message.ThreadId,
                message.Id,
                enabledChannels,
                cancellationToken);
        }
    }

    private async Task TrySendOfflineEmailAsync(Guid recipientUserId, Message message, CancellationToken cancellationToken)
    {
        User? recipient = await _userAuthenticationRepository.FindByIdAsync(recipientUserId, cancellationToken);
        if (recipient is null)
        {
            return;
        }

        Dictionary<string, string> metadata = new(StringComparer.Ordinal)
        {
            ["userId"] = recipientUserId.ToString(),
            ["threadId"] = message.ThreadId.ToString(),
            ["messageId"] = message.Id.ToString()
        };

        await _notificationService.SendAsync(
            new NotificationMessage(
                NotificationChannel.Email,
                recipient.Email.Value,
                "You have a new message",
                BuildMessagePreviewBody(message.Content),
                metadata),
            cancellationToken);
    }

    private static string BuildMessagePreviewBody(string content)
    {
        string normalized = string.IsNullOrWhiteSpace(content) ? "You received a new message." : content.Trim();
        if (normalized.Length <= 160)
        {
            return normalized;
        }

        return $"{normalized[..157]}...";
    }
}
