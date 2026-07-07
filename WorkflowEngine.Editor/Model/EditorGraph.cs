namespace WorkflowEngine.Editor.Model;

/// <summary>
/// Editor-seitige Sicht auf einen Workflow-Step. Getrennt vom Runtime-Config-Modell, damit der
/// Canvas frei mutiert werden kann, ohne bei jedem Zwischenschritt ein valides
/// <c>WorkflowConfigurationModel</c> zu benötigen. <see cref="Id"/> entspricht dem Step-Namen.
/// </summary>
public sealed class EditorNode
{
  public required string Id { get; set; }

  /// <summary>Alias eines <see cref="WorkflowEngine.IWorkflowStep"/>-Bausteins. Optional.</summary>
  public string? ExecuteAlias { get; set; }

  /// <summary>Alias eines Page-Bausteins. Optional.</summary>
  public string? ComponentAlias { get; set; }

  public string? Before { get; set; }

  public string? After { get; set; }

  public int Retry { get; set; }

  public string? OnException { get; set; }

  public string? OnExceptionNextStep { get; set; }

  public double X { get; set; }

  public double Y { get; set; }
}

/// <summary>
/// Eine Kante zwischen zwei Nodes. <see cref="Outcome"/> ist <c>null</c> für die unbedingte
/// Fortsetzung (entspricht <c>Next</c> im Config-Modell); ein gesetzter Wert entspricht einer
/// benannten Transition.
/// </summary>
public sealed class EditorEdge
{
  public required string FromNodeId { get; set; }

  public string? Outcome { get; set; }

  public required string ToNodeId { get; set; }

  public bool IsUnconditional => string.IsNullOrEmpty(Outcome);
}

public sealed class EditorGraph
{
  public string Name { get; set; } = string.Empty;

  public string? StartNodeId { get; set; }

  public List<EditorNode> Nodes { get; set; } = [];

  public List<EditorEdge> Edges { get; set; } = [];

  /// <summary>Entfernt einen Node samt aller ein-/ausgehenden Kanten; rückt den StartNode nach, falls nötig.</summary>
  public void RemoveNode(string nodeId)
  {
    Nodes.RemoveAll(n => n.Id == nodeId);
    Edges.RemoveAll(e => e.FromNodeId == nodeId || e.ToNodeId == nodeId);

    if (StartNodeId == nodeId)
    {
      StartNodeId = Nodes.FirstOrDefault()?.Id;
    }
  }
}
