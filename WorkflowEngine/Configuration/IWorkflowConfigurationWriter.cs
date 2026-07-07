namespace WorkflowEngine.Configuration;

/// <summary>
/// Schreibt ein <see cref="WorkflowConfigurationModel"/> in ein konkretes Format (z.B. JSON).
/// Spiegelbild zu <see cref="IWorkflowConfigurationReader"/>; ein Reader desselben Formats muss
/// das Ergebnis verlustfrei wieder einlesen können.
/// </summary>
public interface IWorkflowConfigurationWriter
{
  /// <summary>Schreibt die Konfiguration als String.</summary>
  string Write(WorkflowConfigurationModel model);

  /// <summary>Schreibt die Konfiguration in einen Stream.</summary>
  void Write(Stream target, WorkflowConfigurationModel model);
}
