using WorkflowEngine.Catalog;
using WorkflowEngine.Editor.Model;
using WorkflowEngine.Validation;

namespace WorkflowEngine.Editor.Components.Editor;

/// <summary>Fertig aufgelöste Darstellungsdaten eines Nodes, damit <c>BlockNode</c> keine
/// Katalog-Lookups selbst durchführen muss.</summary>
public sealed class NodeViewModel
{
  /// <summary>Sentinel-Outcome-Name für die synthetische "Weiter"-Verbindung (entspricht <c>Next</c>).</summary>
  public const string NextOutcomeSentinel = "__next__";

  public required EditorNode Node { get; init; }
  public required string Title { get; init; }
  public required string Subtitle { get; init; }
  public required IReadOnlyList<string> InputLabels { get; init; }
  public required IReadOnlyList<OutcomePort> OutcomePorts { get; init; }
  public required bool HasError { get; init; }
  public required bool HasWarning { get; init; }

  public double Width => NodeMetrics.Width;
  public double Height => NodeMetrics.GetHeight(InputLabels.Count, OutcomePorts.Count);
}

public static class NodeViewModelBuilder
{
  public static List<NodeViewModel> Build(EditorGraph graph, IBlockCatalog catalog, IReadOnlyList<ValidationIssue> issues)
  {
    var result = new List<NodeViewModel>();

    foreach (var node in graph.Nodes)
    {
      var execDescriptor = TryGet(node.ExecuteAlias, catalog);
      var compDescriptor = TryGet(node.ComponentAlias, catalog);

      var inputs = new List<string>();
      inputs.AddRange((execDescriptor?.Inputs ?? []).Select(FormatPort));
      inputs.AddRange((compDescriptor?.Inputs ?? []).Select(FormatPort));

      var outcomeSource = execDescriptor ?? compDescriptor;
      var realOutcomes = outcomeSource?.Outcomes.Where(o => !o.IsImplicit).ToList() ?? [];
      var outcomePorts = realOutcomes.Count > 0
        ? realOutcomes
        : [new OutcomePort(NodeViewModel.NextOutcomeSentinel, "Weiter")];

      var title = compDescriptor?.DisplayName ?? execDescriptor?.DisplayName ?? node.Id;
      var subtitle = BuildSubtitle(node);

      result.Add(new NodeViewModel
      {
        Node = node,
        Title = title,
        Subtitle = subtitle,
        InputLabels = inputs,
        OutcomePorts = outcomePorts,
        HasError = issues.Any(i => i.StepName == node.Id && i.Severity == ValidationSeverity.Error),
        HasWarning = issues.Any(i => i.StepName == node.Id && i.Severity == ValidationSeverity.Warning)
      });
    }

    return result;
  }

  private static string FormatPort(ContextPort port) => port.Required ? port.Key : $"{port.Key} (optional)";

  private static string BuildSubtitle(EditorNode node)
  {
    if (node.ExecuteAlias is not null && node.ComponentAlias is not null)
    {
      return $"{node.ComponentAlias} + {node.ExecuteAlias}";
    }

    return node.ExecuteAlias ?? node.ComponentAlias ?? "(kein Baustein)";
  }

  private static BlockDescriptor? TryGet(string? alias, IBlockCatalog catalog)
  {
    if (string.IsNullOrWhiteSpace(alias))
    {
      return null;
    }

    return catalog.TryGet(alias, out var descriptor) ? descriptor : null;
  }
}
