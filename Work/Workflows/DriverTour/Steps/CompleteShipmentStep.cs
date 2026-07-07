using WorkflowEngine;
using WorkflowEngine.Catalog;

namespace Work.Workflows.DriverTour.Steps;

/// <summary>
/// Schließt die aktuell in Bearbeitung befindliche Einzelsendung ab (Einzelbearbeitung)
/// und entscheidet, ob weitere Sendungen am Stopp offen sind oder der Stopp abgeschlossen werden kann.
/// </summary>
[WorkflowBlock(DisplayName = "Einzelsendung abschließen", Category = "Fahrer-Tour", Description = "Schließt die aktuell bearbeitete Einzelsendung ab.")]
[ConsumesContext("Tour", typeof(Tour))]
[ConsumesContext("Tour.CurrentStopIndex", typeof(int))]
[ConsumesContext("Tour.CurrentShipmentId", typeof(string))]
[ConsumesContext("Shipment.PendingStatus", typeof(ShipmentStatus))]
[ConsumesContext("Shipment.PendingSignature", typeof(string), Required = false)]
[Outcome("StopComplete", DisplayName = "Stopp abschließen")]
[Outcome("ShipmentSelection", DisplayName = "Weitere Sendungen bearbeiten")]
public sealed class CompleteShipmentStep : IWorkflowStep
{
  public Task<StepResult> ExecuteAsync(WorkflowContext context)
  {
    var tour = context.Get<Tour>("Tour");
    var stopIndex = context.Get<int>("Tour.CurrentStopIndex");
    var stop = tour.Stops[stopIndex];

    var shipmentId = context.Get<string>("Tour.CurrentShipmentId");
    var shipment = stop.Shipments.First(s => s.Id == shipmentId);

    var status = context.Get<ShipmentStatus>("Shipment.PendingStatus");
    context.TryGet<string>("Shipment.PendingSignature", out var signature);

    shipment.Status = status;
    shipment.Signature = signature;
    shipment.IsCompleted = true;
    shipment.CompletedAtUtc = DateTime.UtcNow;

    context.Data.Remove("Shipment.PendingStatus");
    context.Data.Remove("Shipment.PendingSignature");
    context.Data.Remove("Tour.CurrentShipmentId");

    var outcome = stop.AllShipmentsCompleted ? "StopComplete" : "ShipmentSelection";
    return Task.FromResult(StepResult.Branch(outcome, result: shipment));
  }
}
