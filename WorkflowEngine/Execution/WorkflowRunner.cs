using Microsoft.Extensions.DependencyInjection;
using WorkflowEngine.Definition;
using WorkflowEngine.Persistence;

namespace WorkflowEngine.Execution;

public sealed class WorkflowRunner
{
  private readonly List<string> _history = new();

  public WorkflowRunner(WorkflowDefinition definition, WorkflowContext context)
  {
    Definition = definition ?? throw new ArgumentNullException(nameof(definition));
    Context = context ?? throw new ArgumentNullException(nameof(context));
  }

  public WorkflowRunner(WorkflowDefinition definition, WorkflowContext context, WorkflowSnapshot snapshot)
    : this(definition, context)
  {
    if (snapshot is null)
    {
      throw new ArgumentNullException(nameof(snapshot));
    }

    foreach (var item in snapshot.Data)
    {
      Context.Data[item.Key] = item.Value;
    }

    _history.AddRange(snapshot.History);

    if (!string.IsNullOrWhiteSpace(snapshot.CurrentStepName))
    {
      SetCurrentStep(snapshot.CurrentStepName, trackHistory: _history.Count == 0);
    }
    else
    {
      IsCompleted = true;
    }
  }

  public WorkflowDefinition Definition { get; }

  public WorkflowContext Context { get; }

  public WorkflowStepDefinition? CurrentStep { get; private set; }

  public string? CurrentStepName { get; private set; }

  public bool IsCompleted { get; private set; }

  public object? LastResult { get; private set; }

  public IReadOnlyList<string> History => _history.AsReadOnly();

  public event Action? StateChanged;

  public async Task StartAsync()
  {
    IsCompleted = false;
    LastResult = null;
    _history.Clear();
    SetCurrentStep(Definition.StartStep, trackHistory: true);
    await TraverseAutomaticStepsAsync();
    NotifyStateChanged();
  }

  public async Task AdvanceAsync(string? overrideNextStep = null)
  {
    if (IsCompleted)
    {
      return;
    }

    if (CurrentStep is null)
    {
      throw new InvalidOperationException("Es gibt keinen aktiven Step.");
    }

    await ExecuteAndMoveAsync(overrideNextStep);
    await TraverseAutomaticStepsAsync();
    NotifyStateChanged();
  }

  public Task GoBackAsync()
  {
    if (_history.Count <= 1)
    {
      return Task.CompletedTask;
    }

    _history.RemoveAt(_history.Count - 1);
    var previous = _history[^1];
    SetCurrentStep(previous, trackHistory: false);
    IsCompleted = false;
    NotifyStateChanged();
    return Task.CompletedTask;
  }

  public WorkflowSnapshot CreateSnapshot()
  {
    var data = Context.Data.ToDictionary(kvp => kvp.Key, kvp => kvp.Value, StringComparer.Ordinal);
    var history = _history.ToArray();
    return new WorkflowSnapshot(Definition.Name, CurrentStepName ?? string.Empty, data, history);
  }

  private async Task TraverseAutomaticStepsAsync()
  {
    while (!IsCompleted && CurrentStep is not null && CurrentStep.ComponentType is null)
    {
      await ExecuteAndMoveAsync(null);
    }
  }

  private async Task ExecuteAndMoveAsync(string? overrideNextStep)
  {
    var step = CurrentStep ?? throw new InvalidOperationException("Es gibt keinen aktiven Step.");
    var execution = await ExecuteCurrentStepAsync(step);

    var nextStep = overrideNextStep
      ?? execution.ExceptionNextStep
      ?? execution.StepResult?.NextStep;

    if (string.IsNullOrWhiteSpace(nextStep))
    {
      CurrentStep = null;
      CurrentStepName = null;
      IsCompleted = true;
      return;
    }

    SetCurrentStep(nextStep, trackHistory: true);
  }

  private async Task<(StepResult? StepResult, string? ExceptionNextStep)> ExecuteCurrentStepAsync(WorkflowStepDefinition step)
  {
    try
    {
      if (step.Before is not null)
      {
        await step.Before(Context);
      }

      StepResult? result = null;
      if (step.Execute is not null || step.StepType is not null)
      {
        result = await ExecuteStepWithRetryAsync(step);
        LastResult = result.Result;
        Context.Set($"{step.Name}.Result", result.Result);
      }

      if (step.After is not null)
      {
        await step.After(Context);
      }

      return (result, null);
    }
    catch (Exception ex)
    {
      if (step.OnException is null)
      {
        throw;
      }

      var next = await step.OnException(Context, ex);
      return (StepResult.Fail(ex, next), next);
    }
  }

  private async Task<StepResult> ExecuteStepWithRetryAsync(WorkflowStepDefinition step)
  {
    var attempts = Math.Max(step.RetryCount, 0) + 1;

    for (var attempt = 1; attempt <= attempts; attempt++)
    {
      try
      {
        var result = await ExecuteStepCoreAsync(step);
        return result ?? StepResult.Ok();
      }
      catch when (attempt < attempts)
      {
        // retry
      }
    }

    throw new InvalidOperationException("Step-Ausführung konnte nicht abgeschlossen werden.");
  }

  private async Task<StepResult?> ExecuteStepCoreAsync(WorkflowStepDefinition step)
  {
    if (step.Execute is not null)
    {
      return await step.Execute(Context);
    }

    if (step.StepType is null)
    {
      return null;
    }

    if (!typeof(IWorkflowStep).IsAssignableFrom(step.StepType))
    {
      throw new InvalidOperationException($"Step-Typ '{step.StepType.FullName}' implementiert IWorkflowStep nicht.");
    }

    var instance = ActivatorUtilities.CreateInstance(Context.Services, step.StepType);
    var workflowStep = (IWorkflowStep)instance;
    return await workflowStep.ExecuteAsync(Context);
  }

  private void SetCurrentStep(string stepName, bool trackHistory)
  {
    if (!Definition.Steps.TryGetValue(stepName, out var step))
    {
      throw new InvalidOperationException($"Step '{stepName}' ist im Workflow '{Definition.Name}' nicht vorhanden.");
    }

    CurrentStep = step;
    CurrentStepName = stepName;
    if (trackHistory)
    {
      _history.Add(stepName);
    }
  }

  private void NotifyStateChanged()
  {
    StateChanged?.Invoke();
  }
}
