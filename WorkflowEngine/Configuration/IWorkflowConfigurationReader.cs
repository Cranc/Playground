namespace WorkflowEngine.Configuration;

/// <summary>
/// Liest eine Workflow-Konfiguration aus einem konkreten Format (z.B. JSON) in das
/// format-neutrale <see cref="WorkflowConfigurationModel"/>. Weitere Formate (z.B. XML)
/// implementieren dieselbe Schnittstelle und liefern dasselbe Modell.
/// </summary>
public interface IWorkflowConfigurationReader
{
  /// <summary>Liest die Konfiguration aus einem Stream.</summary>
  WorkflowConfigurationModel Read(Stream content);

  /// <summary>Liest die Konfiguration aus einem String.</summary>
  WorkflowConfigurationModel Read(string content);
}
