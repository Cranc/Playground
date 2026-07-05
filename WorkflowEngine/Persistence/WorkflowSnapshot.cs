namespace WorkflowEngine.Persistence;

public sealed record WorkflowSnapshot(
  string WorkflowName,
  string CurrentStepName,
  IReadOnlyDictionary<string, object?> Data,
  IReadOnlyList<string> History);
