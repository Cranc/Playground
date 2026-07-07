namespace WorkflowEngine.Configuration;

/// <summary>
/// Serialisierbare, format-neutrale Beschreibung eines Workflows. Wird von einem
/// <see cref="IWorkflowConfigurationReader"/> (z.B. aus JSON) befüllt und anschließend
/// von der <see cref="WorkflowDefinitionFactory"/> in eine ausführbare
/// <see cref="Definition.WorkflowDefinition"/> übersetzt.
/// </summary>
public sealed class WorkflowConfigurationModel
{
  /// <summary>Name des Workflows.</summary>
  public string Name { get; set; } = string.Empty;

  /// <summary>Name des Steps, mit dem der Workflow startet.</summary>
  public string StartStep { get; set; } = string.Empty;

  /// <summary>Alle Steps des Workflows in Definitionsreihenfolge.</summary>
  public List<WorkflowStepConfigurationModel> Steps { get; set; } = new();
}

/// <summary>
/// Beschreibung eines einzelnen Workflow-Steps. Konkrete .NET-Typen werden nicht direkt,
/// sondern über einen stabilen Alias referenziert, der zur Laufzeit von der
/// <see cref="IWorkflowTypeRegistry"/> aufgelöst wird.
/// </summary>
public sealed class WorkflowStepConfigurationModel
{
  /// <summary>Eindeutiger Name des Steps innerhalb des Workflows.</summary>
  public string Name { get; set; } = string.Empty;

  /// <summary>Alias einer Aktion, die vor der Ausführung des Steps läuft (z.B. Kontext initialisieren). Optional.</summary>
  public string? Before { get; set; }

  /// <summary>Alias eines <see cref="IWorkflowStep"/>-Typs, der ausgeführt wird. Optional.</summary>
  public string? Execute { get; set; }

  /// <summary>Alias eines Blazor-Component-Typs, der die UI des Steps rendert. Optional.</summary>
  public string? Component { get; set; }

  /// <summary>Alias einer Aktion, die nach der Ausführung des Steps läuft. Optional.</summary>
  public string? After { get; set; }

  /// <summary>Anzahl zusätzlicher Wiederholungen bei Fehlern (0 = keine Wiederholung).</summary>
  public int Retry { get; set; }

  /// <summary>
  /// Alias eines Exception-Handlers, der bei einem Fehler ausgeführt wird und optional den
  /// nächsten Step als Rückgabewert bestimmt. Optional. Hat Vorrang vor
  /// <see cref="OnExceptionNextStep"/>.
  /// </summary>
  public string? OnException { get; set; }

  /// <summary>
  /// Deklarativer Zielstep, zu dem bei einer Exception gesprungen wird. Optional.
  /// Bequemer Kurzweg, wenn kein eigener Handler (<see cref="OnException"/>) nötig ist.
  /// </summary>
  public string? OnExceptionNextStep { get; set; }

  /// <summary>Ordnet benannte Outcomes (<see cref="StepResult.Outcome"/>) den jeweiligen Ziel-Steps zu. Optional.</summary>
  public Dictionary<string, string> Transitions { get; set; } = new();

  /// <summary>
  /// Unbedingter Ziel-Step, falls weder Outcome noch <see cref="StepResult.NextStep"/> einen Step liefern. Optional.
  /// </summary>
  public string? Next { get; set; }
}
