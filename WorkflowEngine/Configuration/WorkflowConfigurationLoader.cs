using WorkflowEngine.Definition;

namespace WorkflowEngine.Configuration;

/// <summary>
/// Komfort-Fassade, die das Lesen der Konfiguration (<see cref="IWorkflowConfigurationReader"/>)
/// und die Erzeugung der <see cref="WorkflowDefinition"/> (<see cref="WorkflowDefinitionFactory"/>)
/// in einem Aufruf bündelt.
/// </summary>
public sealed class WorkflowConfigurationLoader
{
  private readonly IWorkflowConfigurationReader _reader;
  private readonly WorkflowDefinitionFactory _factory;

  /// <summary>Erzeugt einen Loader mit JSON-Reader als Standard.</summary>
  public WorkflowConfigurationLoader()
    : this(new JsonWorkflowConfigurationReader())
  {
  }

  /// <summary>Erzeugt einen Loader mit einem beliebigen Reader (z.B. für XML).</summary>
  public WorkflowConfigurationLoader(IWorkflowConfigurationReader reader)
  {
    _reader = reader ?? throw new ArgumentNullException(nameof(reader));
    _factory = new WorkflowDefinitionFactory();
  }

  /// <summary>Lädt eine Definition aus einem Konfigurations-String (JSON per Default-Reader).</summary>
  public WorkflowDefinition Load(string content, IWorkflowRegistry registry)
  {
    var model = _reader.Read(content);
    return _factory.Create(model, registry);
  }

  /// <summary>Lädt eine Definition aus einem Stream.</summary>
  public WorkflowDefinition Load(Stream content, IWorkflowRegistry registry)
  {
    var model = _reader.Read(content);
    return _factory.Create(model, registry);
  }

  /// <summary>Lädt eine Definition aus einer Datei.</summary>
  public WorkflowDefinition LoadFromFile(string path, IWorkflowRegistry registry)
  {
    if (string.IsNullOrWhiteSpace(path))
    {
      throw new ArgumentException("Pfad darf nicht leer sein.", nameof(path));
    }

    using var stream = File.OpenRead(path);
    return Load(stream, registry);
  }
}
