using System.Text.Json;
using System.Text.Json.Nodes;

namespace WorkflowEngine.Configuration;

/// <summary>
/// Schreibt eine Workflow-Konfiguration als JSON, camelCase, eingerückt. Leere Transitions,
/// unbenutzte Felder (Before/Execute/Component/After/OnException/Next) und Retry=0 werden
/// weggelassen, damit das Ergebnis genauso kompakt bleibt wie handgeschriebene Konfigurationen
/// und <see cref="JsonWorkflowConfigurationReader"/> es verlustfrei zurückliest.
/// </summary>
public sealed class JsonWorkflowConfigurationWriter : IWorkflowConfigurationWriter
{
  private static readonly JsonSerializerOptions Options = new()
  {
    WriteIndented = true
  };

  /// <inheritdoc />
  public string Write(WorkflowConfigurationModel model)
  {
    if (model is null)
    {
      throw new ArgumentNullException(nameof(model));
    }

    return ToJson(model).ToJsonString(Options);
  }

  /// <inheritdoc />
  public void Write(Stream target, WorkflowConfigurationModel model)
  {
    if (target is null)
    {
      throw new ArgumentNullException(nameof(target));
    }

    if (model is null)
    {
      throw new ArgumentNullException(nameof(model));
    }

    using var writer = new Utf8JsonWriter(target, new JsonWriterOptions { Indented = true });
    ToJson(model).WriteTo(writer, Options);
  }

  private static JsonObject ToJson(WorkflowConfigurationModel model)
  {
    var steps = new JsonArray();
    foreach (var step in model.Steps)
    {
      steps.Add(ToJson(step));
    }

    return new JsonObject
    {
      ["name"] = model.Name,
      ["startStep"] = model.StartStep,
      ["steps"] = steps
    };
  }

  private static JsonObject ToJson(WorkflowStepConfigurationModel step)
  {
    var node = new JsonObject
    {
      ["name"] = step.Name
    };

    AddIfPresent(node, "before", step.Before);
    AddIfPresent(node, "execute", step.Execute);
    AddIfPresent(node, "component", step.Component);
    AddIfPresent(node, "after", step.After);

    if (step.Retry > 0)
    {
      node["retry"] = step.Retry;
    }

    AddIfPresent(node, "onException", step.OnException);
    AddIfPresent(node, "onExceptionNextStep", step.OnExceptionNextStep);

    if (step.Transitions.Count > 0)
    {
      var transitions = new JsonObject();
      foreach (var (outcome, target) in step.Transitions)
      {
        transitions[outcome] = target;
      }

      node["transitions"] = transitions;
    }

    AddIfPresent(node, "next", step.Next);

    return node;
  }

  private static void AddIfPresent(JsonObject node, string propertyName, string? value)
  {
    if (!string.IsNullOrWhiteSpace(value))
    {
      node[propertyName] = value;
    }
  }
}
