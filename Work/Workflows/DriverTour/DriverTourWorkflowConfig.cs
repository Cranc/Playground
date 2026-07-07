using WorkflowEngine.Catalog;
using WorkflowEngine.Configuration;

namespace Work.Workflows.DriverTour;

/// <summary>
/// Erstellt die für den DriverTour-Workflow benötigte <see cref="WorkflowTypeRegistry"/> aus
/// dem per Assembly-Scan gewonnenen <see cref="IBlockCatalog"/>. Die Aliase entsprechen den
/// Klassennamen und damit den in <c>DriverTourWorkflow.json</c> verwendeten Bezeichnern.
/// </summary>
public static class DriverTourWorkflowConfig
{
  /// <summary>Relativer Pfad der JSON-Konfiguration (wird ins Ausgabeverzeichnis kopiert).</summary>
  public const string ConfigFileRelativePath = @"wwwroot\json\DriverTourWorkflow.json";

  private static readonly Lazy<IBlockCatalog> Catalog =
    new(() => new BlockCatalogBuilder().Build(typeof(DriverTourWorkflowConfig).Assembly));

  /// <summary>Erstellt eine Registry mit allen DriverTour-Typen aus dem Baustein-Katalog.</summary>
  public static WorkflowTypeRegistry CreateRegistry()
  {
    return WorkflowTypeRegistry.FromCatalog(Catalog.Value);
  }

  /// <summary>Absoluter Pfad zur JSON-Konfiguration im Ausgabeverzeichnis.</summary>
  public static string ConfigFilePath =>
    Path.Combine(AppContext.BaseDirectory, ConfigFileRelativePath);
}
