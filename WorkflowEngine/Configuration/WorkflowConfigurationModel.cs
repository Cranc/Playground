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

  /// <summary>Alias eines <see cref="IWorkflowStep"/>-Typs, der ausgeführt wird. Optional.</summary>
  public string? Execute { get; set; }

  /// <summary>Alias eines Blazor-Component-Typs, der die UI des Steps rendert. Optional.</summary>
  public string? Component { get; set; }

  /// <summary>Anzahl zusätzlicher Wiederholungen bei Fehlern (0 = keine Wiederholung).</summary>
  public int Retry { get; set; }

  /// <summary>
  /// Deklarativer Zielstep, zu dem bei einer Exception gesprungen wird. Optional.
  /// Ersetzt für den einfachen Fall einen selbstgeschriebenen OnException-Delegate.
  /// </summary>
  public string? OnExceptionNextStep { get; set; }
}
