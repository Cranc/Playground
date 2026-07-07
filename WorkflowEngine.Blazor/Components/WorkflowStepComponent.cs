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

  protected Task GoBackAsync()
  {
    return Runner.GoBackAsync();
  }
}
