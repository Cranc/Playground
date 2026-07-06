namespace WorkflowEngine.Definition;

public sealed class WorkflowDefinition
{
  public WorkflowDefinition(string name, string startStep, IReadOnlyDictionary<string, WorkflowStepDefinition> steps)
  {
    Name = name;
    StartStep = startStep;
    Steps = steps;
  }

  public string Name { get; }

  public string StartStep { get; }

  public IReadOnlyDictionary<string, WorkflowStepDefinition> Steps { get; }
}
