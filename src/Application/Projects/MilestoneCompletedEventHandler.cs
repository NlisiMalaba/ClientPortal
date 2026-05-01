using Application.Abstractions;
using Application.Clients.Abstractions;
using Application.Notifications.Abstractions;
using Application.Projects.Abstractions;
using Domain;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Projects;

public sealed class MilestoneCompletedEventHandler : INotificationHandler<MilestoneCompletedEvent>
{
    private readonly IMilestoneRepository _milestoneRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IClientRepository _clientRepository;
    private readonly ICurrentTenant _currentTenant;
    private readonly INotificationService _notificationService;
    private readonly ILogger<MilestoneCompletedEventHandler> _logger;

    public MilestoneCompletedEventHandler(
        IMilestoneRepository milestoneRepository,
        IProjectRepository projectRepository,
        IClientRepository clientRepository,
        ICurrentTenant currentTenant,
        INotificationService notificationService,
        ILogger<MilestoneCompletedEventHandler> logger)
    {
        _milestoneRepository = milestoneRepository;
        _projectRepository = projectRepository;
        _clientRepository = clientRepository;
        _currentTenant = currentTenant;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task Handle(MilestoneCompletedEvent notification, CancellationToken cancellationToken)
    {
        Milestone? milestone = await _milestoneRepository.FindByIdAsync(notification.MilestoneId, cancellationToken);
        if (milestone is null || milestone.ProjectId != notification.ProjectId)
        {
            _logger.LogWarning(
                "MilestoneCompletedEvent ignored because milestone {MilestoneId} was not found for project {ProjectId}.",
                notification.MilestoneId,
                notification.ProjectId);
            return;
        }

        Project? project = await _projectRepository.FindByIdAsync(notification.ProjectId, cancellationToken);
        if (project is null)
        {
            _logger.LogWarning(
                "MilestoneCompletedEvent ignored because project {ProjectId} was not found.",
                notification.ProjectId);
            return;
        }

        Client? client = await _clientRepository.FindByIdAsync(project.ClientId, cancellationToken);
        if (client is null)
        {
            _logger.LogWarning(
                "MilestoneCompletedEvent ignored because client {ClientId} was not found for project {ProjectId}.",
                project.ClientId,
                project.Id);
            return;
        }

        (NotificationChannel channel, string recipient) = ResolvePreferredChannel(client);
        string subject = $"Milestone completed: {milestone.Name}";
        string body =
            $"Hello {client.ContactName},\n\n" +
            $"Milestone \"{milestone.Name}\" for project \"{project.Name}\" has been completed.\n" +
            $"Completed at: {notification.CompletedAt:yyyy-MM-dd HH:mm:ss} UTC.";

        try
        {
            await _notificationService.SendAsync(
                new NotificationMessage(channel, recipient, subject, body),
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send milestone completion notification for milestone {MilestoneId}.",
                milestone.Id);
        }
    }

    private (NotificationChannel Channel, string Recipient) ResolvePreferredChannel(Client client)
    {
        string[] preferredChannels = (_currentTenant.Settings?.NotificationChannels ?? ["email"])
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .ToArray();

        foreach (string channel in preferredChannels)
        {
            if (channel.Equals("email", StringComparison.OrdinalIgnoreCase))
            {
                return (NotificationChannel.Email, client.Email.Value);
            }

            if (channel.Equals("whatsapp", StringComparison.OrdinalIgnoreCase))
            {
                return (NotificationChannel.WhatsApp, client.Phone.Value);
            }
        }

        return (NotificationChannel.Email, client.Email.Value);
    }
}
