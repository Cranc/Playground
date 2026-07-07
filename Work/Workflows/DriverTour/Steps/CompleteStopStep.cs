using WorkflowEngine;
using WorkflowEngine.Catalog;

namespace Work.Workflows.DriverTour.Steps;

/// <summary>
/// Markiert den aktuellen Stopp als abgeschlossen, sobald alle Sendungen bearbeitet wurden.
/// Danach folgt automatisch die Lademittelbuchung.
/// </summary>
[WorkflowBlock(DisplayName = "Stopp als abgeschlossen markieren", Category = "Fahrer-Tour", Description = "Markiert den aktuellen Stopp als abgeschlossen.")]
[ConsumesContext("Tour", typeof(Tour))]
[ConsumesContext("Tour.CurrentStopIndex", typeof(int))]
public sealed class CompleteStopStep : IWorkflowStep
{
  public Task<StepResult> ExecuteAsync(WorkflowContext context)
  {
    var tour = context.Get<Tour>("Tour");
    var stopIndex = context.Get<int>("Tour.CurrentStopIndex");
    var stop = tour.Stops[stopIndex];

    stop.IsCompleted = true;

    return Task.FromResult(StepResult.Ok());
  }
}
