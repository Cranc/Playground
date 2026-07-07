using WorkflowEngine.Blazor;
using WorkflowEngine.Configuration;
using WorkflowEngine.Definition;
using Work.Workflows.DriverTour.Pages;
using Work.Workflows.DriverTour.Steps;

namespace Work.Workflows.DriverTour;

/// <summary>
/// Bildet den Arbeitsablauf eines Logistikfahrers ab:
/// Tourstart -> Ankunft am Stopp -> (Stop-Abschluss | Einzelbearbeitung je Sendung)
/// -> Abschluss des Stopps -> Lademittelbuchung -> Weiterfahrt (Schleife) -> Tourende.
/// </summary>
public static class DriverTourWorkflow
{
  public static WorkflowDefinition Build()
  {
    return new WorkflowBuilder("DriverTour")
      .StartWith("TourStart")
      .Step("TourStart")
        .Execute<InitializeTourStep>()
        .Next("StopArrival")
      .Step("StopArrival")
        .Component<StopArrivalPage>()
        .On("BulkShipmentStatus", "BulkShipmentStatus")
        .On("ShipmentSelection", "ShipmentSelection")
      .Step("ShipmentSelection")
        .Component<ShipmentSelectionPage>()
        .On("ShipmentStatus", "ShipmentStatus")
        .On("StopComplete", "StopComplete")
      .Step("ShipmentStatus")
        .Component<ShipmentStatusPage>()
        .On("Signature", "Signature")
        .On("CompleteShipment", "CompleteShipment")
      .Step("Signature")
        .Component<SignaturePage>()
        .Next("CompleteShipment")
      .Step("CompleteShipment")
        .Execute<CompleteShipmentStep>()
        .On("StopComplete", "StopComplete")
        .On("ShipmentSelection", "ShipmentSelection")
      .Step("BulkShipmentStatus")
        .Component<BulkShipmentStatusPage>()
        .Next("CompleteBulkShipments")
      .Step("CompleteBulkShipments")
        .Execute<CompleteBulkShipmentsStep>()
        .Next("StopComplete")
      .Step("StopComplete")
        .Execute<CompleteStopStep>()
        .Next("LoadCarrierBooking")
      .Step("LoadCarrierBooking")
        .Component<LoadCarrierBookingPage>()
        .Execute<AdvanceStopStep>()
        .On("StopArrival", "StopArrival")
        .On("TourEnd", "TourEnd")
      .Step("TourEnd")
        .Component<TourEndPage>()
      .Build();
  }

  /// <summary>
  /// Baut denselben Workflow wie <see cref="Build"/>, jedoch aus der externen
  /// JSON-Konfiguration (<c>DriverTourWorkflow.json</c>). Dient als Nachweis, dass eine
  /// komplette Definition auch deklarativ geladen werden kann.
  /// </summary>
  public static WorkflowDefinition BuildFromConfig()
  {
    var registry = DriverTourWorkflowConfig.CreateRegistry();
    var loader = new WorkflowConfigurationLoader();
    return loader.LoadFromFile(DriverTourWorkflowConfig.ConfigFilePath, registry);
  }
}
