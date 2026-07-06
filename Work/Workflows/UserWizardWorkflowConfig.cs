using WorkflowEngine;
using WorkflowEngine.Configuration;
using Work.Workflows.Pages;
using Work.Workflows.Steps;

namespace Work.Workflows;

/// <summary>
/// Registriert alle für den UserWizard-Workflow benötigten Typen sowie die Before-/After-
/// Aktionen und den Exception-Handler. Zeigt, dass auch die komplexe Konfiguration aus
/// <see cref="UserWizardWorkflow.Build"/> (inkl. Kontext-Initialisierung, Zeitstempel und
/// Fehlersprung) vollständig deklarativ aus <c>UserWizardWorkflow.json</c> ladbar ist.
/// </summary>
public static class UserWizardWorkflowConfig
{
  /// <summary>Relativer Pfad der JSON-Konfiguration (wird ins Ausgabeverzeichnis kopiert).</summary>
  public const string ConfigFileRelativePath = @"wwwroot\json\UserWizardWorkflow.json";

  /// <summary>Erstellt eine Registry mit allen UserWizard-Typen und -Hooks.</summary>
  public static WorkflowTypeRegistry CreateRegistry()
  {
    return new WorkflowTypeRegistry()
      .RegisterStep<CreateUserStep>()
      .RegisterStep<FinishStep>()
      .RegisterComponent(typeof(StartPage))
      .RegisterComponent(typeof(SelectUserPage))
      .RegisterComponent(typeof(CreateUserPage))
      .RegisterComponent(typeof(ConfigureRolesPage))
      .RegisterComponent(typeof(SummaryPage))
      .RegisterComponent(typeof(AdvancedPage))
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
