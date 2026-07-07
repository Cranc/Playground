using WorkflowEngine;
using WorkflowEngine.Catalog;

namespace Work.Workflows.DriverTour.Steps;

/// <summary>
/// Übernimmt die auf der Lademittelbuchungs-Seite erfassten Buchungen und fährt entweder
/// zum nächsten Stopp weiter oder beendet die Tour, wenn keine Stopps mehr offen sind.
/// </summary>
[WorkflowBlock(DisplayName = "Stopp abschließen & weiterfahren", Category = "Fahrer-Tour", Description = "Übernimmt Lademittelbuchungen und wechselt zum nächsten Stopp oder beendet die Tour.")]
[ConsumesContext("Tour", typeof(Tour))]
[ConsumesContext("Tour.CurrentStopIndex", typeof(int))]
[ConsumesContext("Stop.PendingLoadCarrierBookings", typeof(List<LoadCarrierBooking>), Required = false)]
[ProducesContext("Tour.CurrentStopIndex", typeof(int))]
[Outcome("StopArrival", DisplayName = "Nächster Stopp")]
[Outcome("TourEnd", DisplayName = "Tour beendet")]
public sealed class AdvanceStopStep : IWorkflowStep
{
  public Task<StepResult> ExecuteAsync(WorkflowContext context)
  {
    var tour = context.Get<Tour>("Tour");
    var stopIndex = context.Get<int>("Tour.CurrentStopIndex");
    var stop = tour.Stops[stopIndex];

    if (context.TryGet<List<LoadCarrierBooking>>("Stop.PendingLoadCarrierBookings", out var bookings) && bookings is not null)
    {
      stop.LoadCarrierBookings.AddRange(bookings);
    }

    context.Data.Remove("Stop.PendingLoadCarrierBookings");

    var nextStopIndex = stopIndex + 1;
    if (nextStopIndex < tour.Stops.Count)
    {
      context.Set("Tour.CurrentStopIndex", nextStopIndex);
      return Task.FromResult(StepResult.Ok("StopArrival"));
    }

    return Task.FromResult(StepResult.Ok("TourEnd"));
  }
}
