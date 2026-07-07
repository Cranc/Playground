using WorkflowEngine;
using WorkflowEngine.Catalog;

namespace Work.Workflows.DriverTour.Steps;

/// <summary>
/// Schließt alle Sendungen des aktuellen Stopps gemeinsam ab (Stop-Abschluss).
/// </summary>
[WorkflowBlock(DisplayName = "Sendungen sammelabschließen", Category = "Fahrer-Tour", Description = "Schließt alle offenen Sendungen eines Stopps gemeinsam mit demselben Status ab.")]
[ConsumesContext("Tour", typeof(Tour))]
[ConsumesContext("Tour.CurrentStopIndex", typeof(int))]
[ConsumesContext("Stop.BulkStatus", typeof(ShipmentStatus))]
[ConsumesContext("Stop.BulkSignature", typeof(string), Required = false)]
public sealed class CompleteBulkShipmentsStep : IWorkflowStep
{
  public Task<StepResult> ExecuteAsync(WorkflowContext context)
  {
    var tour = context.Get<Tour>("Tour");
    var stopIndex = context.Get<int>("Tour.CurrentStopIndex");
    var stop = tour.Stops[stopIndex];

    var status = context.Get<ShipmentStatus>("Stop.BulkStatus");
    context.TryGet<string>("Stop.BulkSignature", out var signature);

    foreach (var shipment in stop.OpenShipments.ToList())
    {
      shipment.Status = status;
      shipment.Signature = ShipmentStatusInfo.RequiresSignature(status) ? signature : null;
      shipment.IsCompleted = true;
      shipment.CompletedAtUtc = DateTime.UtcNow;
    }

    context.Data.Remove("Stop.BulkStatus");
    context.Data.Remove("Stop.BulkSignature");

    return Task.FromResult(StepResult.Ok());
  }
}
