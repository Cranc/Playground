using Microsoft.AspNetCore.Components;
using WorkflowEngine.Execution;

namespace WorkflowEngine.Blazor.Components;

public abstract class WorkflowStepComponent : ComponentBase, IWorkflowPageComponent
{
  [CascadingParameter]
  protected WorkflowRunner Runner { get; set; } = null!;

  [CascadingParameter]
  protected WorkflowContext Context { get; set; } = null!;

  protected Task AdvanceAsync(string? nextStep = null)
  {
    return Runner.AdvanceAsync(nextStep);
  }

  /// <summary>Bewegt den Workflow über einen benannten Outcome weiter (siehe [Outcome]-Attribut).</summary>
  protected Task AdvanceViaAsync(string outcome)
  {
    return Runner.AdvanceByOutcomeAsync(outcome);
  }

  protected Task GoBackAsync()
  {
    return Runner.GoBackAsync();
  }
}
