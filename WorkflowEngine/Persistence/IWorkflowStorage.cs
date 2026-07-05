namespace WorkflowEngine.Persistence;

public interface IWorkflowStorage
{
  Task SaveAsync(string instanceId, WorkflowSnapshot snapshot, CancellationToken cancellationToken = default);

  Task<WorkflowSnapshot?> LoadAsync(string instanceId, CancellationToken cancellationToken = default);

  Task DeleteAsync(string instanceId, CancellationToken cancellationToken = default);
}
