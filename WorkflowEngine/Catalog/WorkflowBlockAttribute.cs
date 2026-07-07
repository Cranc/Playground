namespace WorkflowEngine.Catalog;

/// <summary>
/// Markiert einen <see cref="IWorkflowStep"/>- oder Page-Typ als Baustein, der im
/// <see cref="IBlockCatalog"/> auftaucht. Ohne dieses Attribut wird der Typ trotzdem
/// erfasst, sofern er <see cref="IWorkflowStep"/> implementiert oder von einer Blazor-
/// Komponente erbt; das Attribut dient dann nur der zusätzlichen Beschreibung.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class WorkflowBlockAttribute : Attribute
{
  /// <summary>Alias in der Konfiguration. Default: Klassenname (wie in der Registry).</summary>
  public string? Alias { get; init; }

  public string? DisplayName { get; init; }

  public string Category { get; init; } = "Allgemein";

  public string? Description { get; init; }
}
