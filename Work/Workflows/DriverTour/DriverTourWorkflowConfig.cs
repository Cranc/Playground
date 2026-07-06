using WorkflowEngine.Configuration;
using Work.Workflows.DriverTour.Pages;
using Work.Workflows.DriverTour.Steps;

namespace Work.Workflows.DriverTour;

/// <summary>
/// Registriert die für den DriverTour-Workflow benötigten Step- und Component-Typen in
/// einer <see cref="WorkflowTypeRegistry"/>. Die Aliase entsprechen den Klassennamen und
/// damit den in <c>DriverTourWorkflow.json</c> verwendeten Bezeichnern.
/// </summary>
public static class DriverTourWorkflowConfig
{
  /// <summary>Relativer Pfad der JSON-Konfiguration (wird ins Ausgabeverzeichnis kopiert).</summary>
  public const string ConfigFileRelativePath = @"wwwroot\json\DriverTourWorkflow.json";

  /// <summary>Erstellt eine Registry mit allen DriverTour-Typen.</summary>
  public static WorkflowTypeRegistry CreateRegistry()
  {
    return new WorkflowTypeRegistry()
      .RegisterStep<InitializeTourStep>()
      .RegisterStep<CompleteShipmentStep>()
      .RegisterStep<CompleteBulkShipmentsStep>()
      .RegisterStep<CompleteStopStep>()
      .RegisterStep<AdvanceStopStep>()
      .RegisterComponent(typeof(StopArrivalPage))
      .RegisterComponent(typeof(ShipmentSelectionPage))
      .RegisterComponent(typeof(ShipmentStatusPage))
      .RegisterComponent(typeof(SignaturePage))
      .RegisterComponent(typeof(BulkShipmentStatusPage))
      .RegisterComponent(typeof(LoadCarrierBookingPage))
      .RegisterComponent(typeof(TourEndPage));
  }

  /// <summary>Absoluter Pfad zur JSON-Konfiguration im Ausgabeverzeichnis.</summary>
  public static string ConfigFilePath =>
    Path.Combine(AppContext.BaseDirectory, ConfigFileRelativePath);
}
