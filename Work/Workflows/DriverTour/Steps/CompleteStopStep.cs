using WorkflowEngine;

namespace Work.Workflows.DriverTour.Steps;

/// <summary>
/// Markiert den aktuellen Stopp als abgeschlossen, sobald alle Sendungen bearbeitet wurden.
/// Danach folgt automatisch die Lademittelbuchung.
/// </summary>
public sealed class CompleteStopStep : IWorkflowStep
{
  public Task<StepResult> ExecuteAsync(WorkflowContext context)
  {
    var tour = context.Get<Tour>("Tour");
    var stopIndex = context.Get<int>("Tour.CurrentStopIndex");
    var stop = tour.Stops[stopIndex];

    stop.IsCompleted = true;

    return Task.FromResult(StepResult.Ok("LoadCarrierBooking"));
  }
}
