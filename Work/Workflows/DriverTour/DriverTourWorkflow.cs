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
      .Step("StopArrival")
        .Component<StopArrivalPage>()
      .Step("ShipmentSelection")
        .Component<ShipmentSelectionPage>()
      .Step("ShipmentStatus")
        .Component<ShipmentStatusPage>()
      .Step("Signature")
        .Component<SignaturePage>()
      .Step("CompleteShipment")
        .Execute<CompleteShipmentStep>()
      .Step("BulkShipmentStatus")
        .Component<BulkShipmentStatusPage>()
      .Step("CompleteBulkShipments")
        .Execute<CompleteBulkShipmentsStep>()
      .Step("StopComplete")
        .Execute<CompleteStopStep>()
      .Step("LoadCarrierBooking")
        .Component<LoadCarrierBookingPage>()
        .Execute<AdvanceStopStep>()
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
