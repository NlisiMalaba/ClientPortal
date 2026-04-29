using Domain;

namespace Application.Projects.Abstractions;

public interface IProjectTaskRepository
{
    Task<ProjectTask?> FindByIdAsync(Guid taskId, CancellationToken cancellationToken = default);

    void Add(ProjectTask task);

    void Update(ProjectTask task);
}
