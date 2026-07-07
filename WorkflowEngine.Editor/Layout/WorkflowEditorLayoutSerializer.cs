using System.Text.Json;

namespace WorkflowEngine.Editor.Layout;

/// <summary>Liest/schreibt die Layout-Sidecar-Datei als JSON (camelCase, eingerückt).</summary>
public static class WorkflowEditorLayoutSerializer
{
  private static readonly JsonSerializerOptions Options = new()
  {
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true
  };

  public static string Write(WorkflowEditorLayout layout)
  {
    if (layout is null)
    {
      throw new ArgumentNullException(nameof(layout));
    }

    return JsonSerializer.Serialize(layout, Options);
  }

  public static WorkflowEditorLayout Read(string json)
  {
    if (string.IsNullOrWhiteSpace(json))
    {
      throw new ArgumentException("Layout-JSON darf nicht leer sein.", nameof(json));
    }

    return JsonSerializer.Deserialize<WorkflowEditorLayout>(json, Options)
      ?? throw new InvalidOperationException("Layout-JSON konnte nicht gelesen werden (null).");
  }
}
