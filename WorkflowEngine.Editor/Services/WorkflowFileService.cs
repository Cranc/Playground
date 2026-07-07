using WorkflowEngine.Configuration;
using WorkflowEngine.Editor.Layout;

namespace WorkflowEngine.Editor.Services;

public interface IWorkflowFileService
{
  /// <summary>Listet die Namen aller Workflow-JSON-Dateien im konfigurierten Verzeichnis (ohne *.layout.json).</summary>
  IReadOnlyList<string> ListWorkflowNames();

  (WorkflowConfigurationModel Model, WorkflowEditorLayout? Layout) Load(string name);

  void Save(string name, WorkflowConfigurationModel model, WorkflowEditorLayout layout);
}

/// <summary>
/// Liest/schreibt Workflow-Konfiguration (<c>{name}.json</c>) und Editor-Layout
/// (<c>{name}.layout.json</c>) in einem gemeinsamen Verzeichnis (typischerweise
/// <c>Work/wwwroot/json</c>).
/// </summary>
public sealed class WorkflowFileService : IWorkflowFileService
{
  private readonly string _directory;
  private readonly JsonWorkflowConfigurationReader _reader = new();
  private readonly JsonWorkflowConfigurationWriter _writer = new();

  public WorkflowFileService(string directory)
  {
    if (string.IsNullOrWhiteSpace(directory))
    {
      throw new ArgumentException("Verzeichnis darf nicht leer sein.", nameof(directory));
    }

    _directory = directory;
  }

  public IReadOnlyList<string> ListWorkflowNames()
  {
    if (!Directory.Exists(_directory))
    {
      return [];
    }

    return Directory.GetFiles(_directory, "*.json")
      .Where(f => !f.EndsWith(".layout.json", StringComparison.OrdinalIgnoreCase))
      .Select(Path.GetFileNameWithoutExtension)
      .Where(n => !string.IsNullOrEmpty(n))
      .Select(n => n!)
      .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
      .ToList();
  }

  public (WorkflowConfigurationModel Model, WorkflowEditorLayout? Layout) Load(string name)
  {
    var jsonPath = GetJsonPath(name);
    if (!File.Exists(jsonPath))
    {
      throw new FileNotFoundException($"Workflow-Konfiguration '{name}' wurde nicht gefunden.", jsonPath);
    }

    var model = _reader.Read(File.ReadAllText(jsonPath));

    var layoutPath = GetLayoutPath(name);
    var layout = File.Exists(layoutPath)
      ? WorkflowEditorLayoutSerializer.Read(File.ReadAllText(layoutPath))
      : null;

    return (model, layout);
  }

  public void Save(string name, WorkflowConfigurationModel model, WorkflowEditorLayout layout)
  {
    if (model is null)
    {
      throw new ArgumentNullException(nameof(model));
    }

    if (layout is null)
    {
      throw new ArgumentNullException(nameof(layout));
    }

    Directory.CreateDirectory(_directory);
    File.WriteAllText(GetJsonPath(name), _writer.Write(model));
    File.WriteAllText(GetLayoutPath(name), WorkflowEditorLayoutSerializer.Write(layout));
  }

  private string GetJsonPath(string name) => Path.Combine(_directory, $"{name}.json");

  private string GetLayoutPath(string name) => Path.Combine(_directory, $"{name}.layout.json");
}
