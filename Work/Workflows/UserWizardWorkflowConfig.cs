using WorkflowEngine;
using WorkflowEngine.Catalog;
using WorkflowEngine.Configuration;

namespace Work.Workflows;

/// <summary>
/// Erstellt die für den UserWizard-Workflow benötigte <see cref="WorkflowTypeRegistry"/> aus
/// dem Baustein-Katalog und ergänzt die Before-/After-Aktionen sowie den Exception-Handler
/// (Delegates ohne Typ-Identität, daher weiterhin manuell registriert). Zeigt, dass auch die
/// komplexe Konfiguration aus <see cref="UserWizardWorkflow.Build"/> (inkl. Kontext-
/// Initialisierung, Zeitstempel und Fehlersprung) vollständig deklarativ aus
/// <c>UserWizardWorkflow.json</c> ladbar ist.
/// </summary>
public static class UserWizardWorkflowConfig
{
  /// <summary>Relativer Pfad der JSON-Konfiguration (wird ins Ausgabeverzeichnis kopiert).</summary>
  public const string ConfigFileRelativePath = @"wwwroot\json\UserWizardWorkflow.json";

  private static readonly Lazy<IBlockCatalog> Catalog =
    new(() => new BlockCatalogBuilder().Build(typeof(UserWizardWorkflowConfig).Assembly));

  /// <summary>Erstellt eine Registry mit allen UserWizard-Typen und -Hooks.</summary>
  public static WorkflowTypeRegistry CreateRegistry()
  {
    return WorkflowTypeRegistry.FromCatalog(Catalog.Value)
      .RegisterAction("CreateUser.InitAttempts", context => context.Set("CreateUser.Attempts", 0))
      .RegisterAction("CreateUser.StampLastRun", context => context.Set("CreateUser.LastRunUtc", DateTime.UtcNow))
      .RegisterExceptionHandler("Finish.HandleError", (context, exception) =>
      {
        context.Set("Finish.Error", exception.Message);
        return "Advanced";
      });
  }

  /// <summary>Absoluter Pfad zur JSON-Konfiguration im Ausgabeverzeichnis.</summary>
  public static string ConfigFilePath =>
    Path.Combine(AppContext.BaseDirectory, ConfigFileRelativePath);
}
