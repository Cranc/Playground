namespace WorkflowEngine.Definition;

public sealed class WorkflowBuilder
{
  private readonly Dictionary<string, WorkflowStepConfiguration> _steps = new(StringComparer.Ordinal);
  private string? _startStep;

  public WorkflowBuilder(string name)
  {
    if (string.IsNullOrWhiteSpace(name))
    {
      throw new ArgumentException("Workflow-Name darf nicht leer sein.", nameof(name));
    }

    Name = name;
  }

  public string Name { get; }

  public WorkflowBuilder StartWith(string stepName)
  {
    if (string.IsNullOrWhiteSpace(stepName))
    {
      throw new ArgumentException("Start-Step darf nicht leer sein.", nameof(stepName));
    }

    _startStep = stepName;
    return this;
  }

  public WorkflowStepBuilder Step(string name)
  {
    if (string.IsNullOrWhiteSpace(name))
    {
      throw new ArgumentException("Step-Name darf nicht leer sein.", nameof(name));
    }

    if (_steps.ContainsKey(name))
    {
      throw new InvalidOperationException($"Step '{name}' ist bereits definiert.");
    }

    var configuration = new WorkflowStepConfiguration(name);
    _steps.Add(name, configuration);
    return new WorkflowStepBuilder(this, configuration);
  }

  public WorkflowDefinition Build()
  {
    if (_steps.Count == 0)
    {
      throw new InvalidOperationException("Mindestens ein Workflow-Step muss definiert sein.");
    }

    if (string.IsNullOrWhiteSpace(_startStep))
    {
      throw new InvalidOperationException("Start-Step wurde nicht festgelegt. Verwende StartWith(...).");
    }

    if (!_steps.ContainsKey(_startStep))
    {
      throw new InvalidOperationException($"Start-Step '{_startStep}' ist nicht als Step definiert.");
    }

    var definitions = _steps.ToDictionary(
      kvp => kvp.Key,
      kvp => kvp.Value.ToDefinition(),
      StringComparer.Ordinal);

    return new WorkflowDefinition(Name, _startStep, definitions);
  }

  internal sealed class WorkflowStepConfiguration
  {
    public WorkflowStepConfiguration(string name)
    {
      Name = name;
    }

    public string Name { get; }
    public Type? StepType { get; set; }
    public Func<WorkflowContext, Task<StepResult>>? Execute { get; set; }
    public Type? ComponentType { get; set; }
    public Func<WorkflowContext, Task>? Before { get; set; }
    public Func<WorkflowContext, Task>? After { get; set; }
    public int RetryCount { get; set; }
    public Func<WorkflowContext, Exception, Task<string?>>? OnException { get; set; }
    public Dictionary<string, string> Transitions { get; } = new(StringComparer.Ordinal);
    public string? Next { get; set; }

    public WorkflowStepDefinition ToDefinition()
    {
      if (StepType is null && Execute is null && ComponentType is null)
      {
        throw new InvalidOperationException($"Step '{Name}' muss mindestens Execute<T>(), Execute(...) oder Component(...) besitzen.");
      }

      return new WorkflowStepDefinition(
        Name,
        StepType,
        Execute,
        ComponentType,
        Before,
        After,
        RetryCount,
        OnException,
        Transitions,
        Next);
    }
  }
}
