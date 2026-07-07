namespace WorkflowEngine.Catalog;

/// <summary>
/// Deklariert, dass ein Baustein einen bestimmten Schlüssel aus dem <see cref="WorkflowContext"/>
/// liest. Wird vom Editor genutzt, um den Datenfluss zwischen Bausteinen zu validieren.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class ConsumesContextAttribute : Attribute
{
  public ConsumesContextAttribute(string key, Type type)
  {
    Key = key;
    Type = type;
  }

  public string Key { get; }

  public Type Type { get; }

  public bool Required { get; init; } = true;
}
