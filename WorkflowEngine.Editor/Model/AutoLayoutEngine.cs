namespace WorkflowEngine.Editor.Model;

/// <summary>
/// Vergibt Positionen für Nodes ohne gespeicherte Layout-Daten: Spalte = BFS-Distanz vom
/// StartNode (Flussrichtung links nach rechts), Zeile = Reihenfolge innerhalb der Spalte.
/// Vom StartNode aus nicht erreichbare Nodes (z.B. gerade erst hinzugefügt) landen in einer
/// eigenen Spalte rechts vom Rest.
/// </summary>
public static class AutoLayoutEngine
{
  public static void ApplyMissingPositions(
    EditorGraph graph,
    IReadOnlySet<string> nodeIdsNeedingLayout,
    double columnSpacing = 260,
    double rowSpacing = 140)
  {
    if (nodeIdsNeedingLayout.Count == 0)
    {
      return;
    }

    var levels = ComputeBfsLevels(graph);
    var fallbackLevel = levels.Count == 0 ? 0 : levels.Values.Max() + 1;
    var rowByLevel = new Dictionary<int, int>();

    foreach (var node in graph.Nodes)
    {
      if (!nodeIdsNeedingLayout.Contains(node.Id))
      {
        continue;
      }

      var level = levels.GetValueOrDefault(node.Id, fallbackLevel);
      var row = rowByLevel.GetValueOrDefault(level, 0);
      rowByLevel[level] = row + 1;

      node.X = (level * columnSpacing) + 40;
      node.Y = (row * rowSpacing) + 40;
    }
  }

  private static Dictionary<string, int> ComputeBfsLevels(EditorGraph graph)
  {
    var levels = new Dictionary<string, int>(StringComparer.Ordinal);

    if (graph.StartNodeId is null || graph.Nodes.All(n => n.Id != graph.StartNodeId))
    {
      return levels;
    }

    var successors = graph.Edges
      .GroupBy(e => e.FromNodeId, StringComparer.Ordinal)
      .ToDictionary(g => g.Key, g => g.Select(e => e.ToNodeId).Distinct().ToList(), StringComparer.Ordinal);

    levels[graph.StartNodeId] = 0;
    var queue = new Queue<string>();
    queue.Enqueue(graph.StartNodeId);

    while (queue.Count > 0)
    {
      var current = queue.Dequeue();
      if (!successors.TryGetValue(current, out var targets))
      {
        continue;
      }

      foreach (var target in targets)
      {
        if (levels.ContainsKey(target))
        {
          continue;
        }

        levels[target] = levels[current] + 1;
        queue.Enqueue(target);
      }
    }

    return levels;
  }
}
