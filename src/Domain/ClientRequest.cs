using Shared;

namespace Domain;

public sealed class ClientRequest : AggregateRoot<Guid>
{
    public Guid ClientId { get; private set; }

    public Guid ProjectId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public ClientRequestStatus Status { get; private set; } = ClientRequestStatus.Submitted;

    public ClientRequestPriority Priority { get; private set; } = ClientRequestPriority.Medium;

    private ClientRequest()
    {
    }

    private ClientRequest(
        Guid id,
        Guid clientId,
        Guid projectId,
        string title,
        string description,
        ClientRequestStatus status,
        ClientRequestPriority priority)
        : base(id)
    {
        ClientId = NormalizeId(clientId, nameof(clientId), "ClientId");
        ProjectId = NormalizeId(projectId, nameof(projectId), "ProjectId");
        Title = NormalizeTitle(title);
        Description = NormalizeDescription(description);
        Status = status;
        Priority = priority;
    }

    public static ClientRequest Create(
        Guid id,
        Guid clientId,
        Guid projectId,
        string title,
        string description,
        ClientRequestStatus status = ClientRequestStatus.Submitted,
        ClientRequestPriority priority = ClientRequestPriority.Medium)
    {
        ClientRequest request = new(id, clientId, projectId, title, description, status, priority);
        request.AddDomainEvent(new ClientRequestSubmittedEvent(request.Id, request.ClientId, request.ProjectId, DateTime.UtcNow));
        return request;
    }

    public void UpdateTitle(string title)
    {
        Title = NormalizeTitle(title);
        MarkUpdated();
    }

    public void UpdateDescription(string description)
    {
        Description = NormalizeDescription(description);
        MarkUpdated();
    }

    public void UpdateStatus(ClientRequestStatus status)
    {
        Status = status;
        MarkUpdated();
    }

    public void UpdatePriority(ClientRequestPriority priority)
    {
        Priority = priority;
        MarkUpdated();
    }

    private static Guid NormalizeId(Guid value, string paramName, string propertyName)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException($"{propertyName} cannot be empty.", paramName);
        }

        return value;
    }

    private static string NormalizeTitle(string title)
    {
        return Guard.NotEmpty(title, nameof(title)).Trim();
    }

    private static string NormalizeDescription(string description)
    {
        return Guard.NotEmpty(description, nameof(description)).Trim();
    }
}
