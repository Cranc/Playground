namespace WorkflowEngine.Definition;

public sealed class WorkflowStepDefinition
{
  public WorkflowStepDefinition(
    string name,
    Type? stepType,
    Func<WorkflowContext, Task<StepResult>>? execute,
    Type? componentType,
    Func<WorkflowContext, Task>? before,
    Func<WorkflowContext, Task>? after,
    int retryCount,
    Func<WorkflowContext, Exception, Task<string?>>? onException,
    IReadOnlyDictionary<string, string>? transitions = null,
    string? next = null)
  {
    Name = name;
    StepType = stepType;
    Execute = execute;
    ComponentType = componentType;
    Before = before;
    After = after;
    RetryCount = retryCount;
    OnException = onException;
    Transitions = transitions ?? new Dictionary<string, string>(StringComparer.Ordinal);
    Next = next;
  }

  public string Name { get; }

  public Type? StepType { get; }

  public Func<WorkflowContext, Task<StepResult>>? Execute { get; }

  public Type? ComponentType { get; }

  public Func<WorkflowContext, Task>? Before { get; }

  public Func<WorkflowContext, Task>? After { get; }

  public int RetryCount { get; }

  public Func<WorkflowContext, Exception, Task<string?>>? OnException { get; }

  /// <summary>Ordnet benannte Outcomes (<see cref="StepResult.Outcome"/>) den jeweiligen Ziel-Steps zu.</summary>
  public IReadOnlyDictionary<string, string> Transitions { get; }

  /// <summary>Unbedingter Ziel-Step, falls weder Override noch Outcome noch <see cref="StepResult.NextStep"/> greifen.</summary>
  public string? Next { get; }
}
