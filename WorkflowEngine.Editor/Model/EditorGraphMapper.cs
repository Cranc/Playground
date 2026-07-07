using WorkflowEngine.Configuration;
using WorkflowEngine.Editor.Layout;

namespace WorkflowEngine.Editor.Model;

/// <summary>Bidirektionale Abbildung zwischen dem Editor-Graphen und Runtime-Config + Layout-Sidecar.</summary>
public static class EditorGraphMapper
{
  public static EditorGraph ToEditorGraph(WorkflowConfigurationModel model, WorkflowEditorLayout? layout)
  {
    if (model is null)
    {
      throw new ArgumentNullException(nameof(model));
    }

    var layoutByStep = layout?.Nodes.ToDictionary(n => n.StepName, n => n, StringComparer.Ordinal)
      ?? new Dictionary<string, NodeLayout>(StringComparer.Ordinal);

    var graph = new EditorGraph
    {
      Name = model.Name,
      StartNodeId = string.IsNullOrWhiteSpace(model.StartStep) ? null : model.StartStep
    };

    foreach (var step in model.Steps)
    {
      var position = layoutByStep.GetValueOrDefault(step.Name);

      graph.Nodes.Add(new EditorNode
      {
        Id = step.Name,
        ExecuteAlias = step.Execute,
        ComponentAlias = step.Component,
        Before = step.Before,
        After = step.After,
        Retry = step.Retry,
        OnException = step.OnException,
        OnExceptionNextStep = step.OnExceptionNextStep,
        X = position?.X ?? 0,
        Y = position?.Y ?? 0
      });

      foreach (var (outcome, target) in step.Transitions)
      {
        graph.Edges.Add(new EditorEdge { FromNodeId = step.Name, Outcome = outcome, ToNodeId = target });
      }

      if (!string.IsNullOrWhiteSpace(step.Next))
      {
        graph.Edges.Add(new EditorEdge { FromNodeId = step.Name, Outcome = null, ToNodeId = step.Next });
      }
    }

    return graph;
  }

  public static (WorkflowConfigurationModel Model, WorkflowEditorLayout Layout) ToConfiguration(EditorGraph graph)
  {
    if (graph is null)
    {
      throw new ArgumentNullException(nameof(graph));
    }

    var model = new WorkflowConfigurationModel
    {
      Name = graph.Name,
      StartStep = graph.StartNodeId ?? string.Empty
    };

    var layoutNodes = new List<NodeLayout>();

    foreach (var node in graph.Nodes)
    {
      var stepModel = new WorkflowStepConfigurationModel
      {
        Name = node.Id,
        Before = node.Before,
        Execute = node.ExecuteAlias,
        Component = node.ComponentAlias,
        After = node.After,
        Retry = node.Retry,
        OnException = node.OnException,
        OnExceptionNextStep = node.OnExceptionNextStep
      };

      foreach (var edge in graph.Edges.Where(e => e.FromNodeId == node.Id))
      {
        if (edge.IsUnconditional)
        {
          stepModel.Next = edge.ToNodeId;
        }
        else
        {
          stepModel.Transitions[edge.Outcome!] = edge.ToNodeId;
        }
      }

      model.Steps.Add(stepModel);
      layoutNodes.Add(new NodeLayout(node.Id, node.X, node.Y));
    }

    return (model, new WorkflowEditorLayout(graph.Name, layoutNodes));
  }
}
