namespace WorkflowEngine.Catalog;

/// <summary>
/// Deklariert einen benannten Ausgang eines Bausteins. Das sind die Ports, an denen der
/// Editor Kanten (Transitions) anhängt.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class OutcomeAttribute : Attribute
{
  public OutcomeAttribute(string name)
  {
    Name = name;
  }

  public string Name { get; }

  public string? DisplayName { get; init; }
}
