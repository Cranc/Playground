namespace WorkflowEngine.Configuration;

/// <summary>
/// Standard-Implementierung von <see cref="IWorkflowTypeRegistry"/>. Step- und
/// Component-Typen werden unter einem Alias registriert; der Default-Alias ist der
/// einfache Klassenname, sodass eine Konfiguration die Typen direkt beim Namen nennen kann.
/// </summary>
public sealed class WorkflowTypeRegistry : IWorkflowTypeRegistry
{
  private readonly Dictionary<string, Type> _steps = new(StringComparer.Ordinal);
  private readonly Dictionary<string, Type> _components = new(StringComparer.Ordinal);

  /// <summary>Registriert einen <see cref="IWorkflowStep"/>-Typ. Alias-Default: Klassenname.</summary>
  public WorkflowTypeRegistry RegisterStep<TStep>(string? alias = null)
    where TStep : class, IWorkflowStep
  {
    return RegisterStep(typeof(TStep), alias);
  }

  /// <summary>Registriert einen <see cref="IWorkflowStep"/>-Typ. Alias-Default: Klassenname.</summary>
  public WorkflowTypeRegistry RegisterStep(Type stepType, string? alias = null)
  {
    if (stepType is null)
    {
      throw new ArgumentNullException(nameof(stepType));
    }

    if (!typeof(IWorkflowStep).IsAssignableFrom(stepType))
    {
      throw new ArgumentException($"Typ '{stepType.FullName}' implementiert IWorkflowStep nicht.", nameof(stepType));
    }

    Add(_steps, ResolveAlias(alias, stepType), stepType, "Step");
    return this;
  }

  /// <summary>Registriert einen Component-Typ. Alias-Default: Klassenname.</summary>
  public WorkflowTypeRegistry RegisterComponent(Type componentType, string? alias = null)
  {
    if (componentType is null)
    {
      throw new ArgumentNullException(nameof(componentType));
    }

    Add(_components, ResolveAlias(alias, componentType), componentType, "Component");
    return this;
  }

  /// <inheritdoc />
  public bool TryResolveStep(string alias, out Type stepType)
  {
    return _steps.TryGetValue(alias, out stepType!);
  }

  /// <inheritdoc />
  public bool TryResolveComponent(string alias, out Type componentType)
  {
    return _components.TryGetValue(alias, out componentType!);
  }

  private static string ResolveAlias(string? alias, Type type)
  {
    return string.IsNullOrWhiteSpace(alias) ? type.Name : alias;
  }

  private static void Add(Dictionary<string, Type> target, string alias, Type type, string kind)
  {
    if (target.ContainsKey(alias))
    {
      throw new InvalidOperationException($"{kind}-Alias '{alias}' ist bereits registriert.");
    }

    target.Add(alias, type);
  }
}
