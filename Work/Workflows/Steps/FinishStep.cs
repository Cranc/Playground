using WorkflowEngine;

namespace Work.Workflows.Steps;

public sealed class FinishStep : IWorkflowStep
{
  public Task<StepResult> ExecuteAsync(WorkflowContext context)
  {
    if (context.TryGet<bool>("Finish.TriggerError", out var triggerError) && triggerError)
    {
      throw new InvalidOperationException("Simulierter Fehler beim finalen Speichern.");
    }

    var summary = new
    {
      CompletedAtUtc = DateTime.UtcNow,
      Roles = context.TryGet<List<string>>("User.Roles", out var roles) ? roles : new List<string>(),
      Path = context.TryGet<string>("Wizard.Path", out var path) ? path : "unknown"
    };

    context.Set("Wizard.Finished", true);
    context.Set("Wizard.Summary", summary);
    return Task.FromResult(StepResult.Ok(result: summary));
  }
}
