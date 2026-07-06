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
    Func<WorkflowContext, Exception, Task<string?>>? onException)
  {
    Name = name;
    StepType = stepType;
    Execute = execute;
    ComponentType = componentType;
    Before = before;
    After = after;
    RetryCount = retryCount;
    OnException = onException;
  }

  public string Name { get; }

  public Type? StepType { get; }

  public Func<WorkflowContext, Task<StepResult>>? Execute { get; }

  public Type? ComponentType { get; }

  public Func<WorkflowContext, Task>? Before { get; }

  public Func<WorkflowContext, Task>? After { get; }

  public int RetryCount { get; }

  public Func<WorkflowContext, Exception, Task<string?>>? OnException { get; }
}
