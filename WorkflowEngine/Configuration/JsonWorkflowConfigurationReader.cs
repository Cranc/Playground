using System.Text.Json;

namespace WorkflowEngine.Configuration;

/// <summary>
/// Liest eine Workflow-Konfiguration aus JSON. Property-Namen werden groß-/kleinschreibungs-
/// tolerant behandelt, sodass camelCase-JSON auf das PascalCase-Modell abgebildet wird.
/// </summary>
public sealed class JsonWorkflowConfigurationReader : IWorkflowConfigurationReader
{
  private static readonly JsonSerializerOptions Options = new()
  {
    PropertyNameCaseInsensitive = true,
    ReadCommentHandling = JsonCommentHandling.Skip,
    AllowTrailingCommas = true
  };

  /// <inheritdoc />
  public WorkflowConfigurationModel Read(Stream content)
  {
    if (content is null)
    {
      throw new ArgumentNullException(nameof(content));
    }

    using var reader = new StreamReader(content);
    return Read(reader.ReadToEnd());
  }

  /// <inheritdoc />
  public WorkflowConfigurationModel Read(string content)
  {
    if (string.IsNullOrWhiteSpace(content))
    {
      throw new ArgumentException("JSON-Inhalt darf nicht leer sein.", nameof(content));
    }

    var model = JsonSerializer.Deserialize<WorkflowConfigurationModel>(content, Options);
    return Validate(model);
  }

  private static WorkflowConfigurationModel Validate(WorkflowConfigurationModel? model)
  {
    return model ?? throw new InvalidOperationException("Die JSON-Konfiguration konnte nicht gelesen werden (null).");
  }
}
