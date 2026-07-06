namespace WorkflowEngine;

public sealed class WorkflowContext
{
  public WorkflowContext(IServiceProvider services, CancellationToken cancellationToken = default)
  {
    Services = services;
    CancellationToken = cancellationToken;
  }

  public IServiceProvider Services { get; }

  public IDictionary<string, object?> Data { get; } = new Dictionary<string, object?>();

  public CancellationToken CancellationToken { get; }

  public T Get<T>(string key)
  {
    if (!Data.TryGetValue(key, out var value))
    {
      throw new KeyNotFoundException($"Der Schlüssel '{key}' wurde im WorkflowContext nicht gefunden.");
    }

    return (T)value!;
  }

  public bool TryGet<T>(string key, out T? value)
  {
    if (Data.TryGetValue(key, out var raw) && raw is T typed)
    {
      value = typed;
      return true;
    }

    value = default;
    return false;
  }

  public void Set<T>(string key, T value)
  {
    Data[key] = value;
  }
}
