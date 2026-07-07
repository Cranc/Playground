namespace WorkflowEngine.Catalog;

/// <summary>
/// Deklariert, dass ein Baustein einen bestimmten Schlüssel im <see cref="WorkflowContext"/>
/// setzt. Wird vom Editor genutzt, um den Datenfluss zwischen Bausteinen zu validieren.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class ProducesContextAttribute : Attribute
{
  public ProducesContextAttribute(string key, Type type)
  {
    Key = key;
    Type = type;
  }

  public string Key { get; }

  public Type Type { get; }
}
