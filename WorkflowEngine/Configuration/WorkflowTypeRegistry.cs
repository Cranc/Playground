namespace WorkflowEngine.Configuration;

/// <summary>
/// Standard-Implementierung von <see cref="IWorkflowRegistry"/>. Step- und Component-Typen
/// werden unter einem Alias registriert (Default-Alias = Klassenname); Before-/After-Aktionen
/// und Exception-Handler werden als benannte Delegates hinterlegt. Damit lässt sich der
/// komplette Funktionsumfang des Fluent-Builders in einer Konfiguration abbilden.
/// </summary>
public sealed class WorkflowTypeRegistry : IWorkflowRegistry
{
  private readonly Dictionary<string, Type> _steps = new(StringComparer.Ordinal);
  private readonly Dictionary<string, Type> _components = new(StringComparer.Ordinal);
  private readonly Dictionary<string, Func<WorkflowContext, Task>> _actions = new(StringComparer.Ordinal);
  private readonly Dictionary<string, Func<WorkflowContext, Exception, Task<string?>>> _exceptionHandlers = new(StringComparer.Ordinal);

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

  /// <summary>Registriert eine Before-/After-Aktion unter einem Alias.</summary>
  public WorkflowTypeRegistry RegisterAction(string alias, Func<WorkflowContext, Task> action)
  {
    if (action is null)
    {
      throw new ArgumentNullException(nameof(action));
    }

    Add(_actions, RequireAlias(alias), action, "Aktions");
    return this;
  }

  /// <summary>Registriert eine synchrone Before-/After-Aktion unter einem Alias.</summary>
  public WorkflowTypeRegistry RegisterAction(string alias, Action<WorkflowContext> action)
  {
    if (action is null)
    {
      throw new ArgumentNullException(nameof(action));
    }

    return RegisterAction(alias, context =>
    {
      action(context);
      return Task.CompletedTask;
    });
  }

  /// <summary>Registriert einen Exception-Handler, der optional den nächsten Step liefert.</summary>
  public WorkflowTypeRegistry RegisterExceptionHandler(string alias, Func<WorkflowContext, Exception, Task<string?>> handler)
  {
    if (handler is null)
    {
      throw new ArgumentNullException(nameof(handler));
    }

    Add(_exceptionHandlers, RequireAlias(alias), handler, "Exception-Handler-");
    return this;
  }

  /// <summary>Registriert einen synchronen Exception-Handler, der optional den nächsten Step liefert.</summary>
  public WorkflowTypeRegistry RegisterExceptionHandler(string alias, Func<WorkflowContext, Exception, string?> handler)
  {
    if (handler is null)
    {
      throw new ArgumentNullException(nameof(handler));
    }

    return RegisterExceptionHandler(alias, (context, exception) => Task.FromResult(handler(context, exception)));
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

  /// <inheritdoc />
  public bool TryResolveAction(string alias, out Func<WorkflowContext, Task> action)
  {
    return _actions.TryGetValue(alias, out action!);
  }

  /// <inheritdoc />
  public bool TryResolveExceptionHandler(string alias, out Func<WorkflowContext, Exception, Task<string?>> handler)
  {
    return _exceptionHandlers.TryGetValue(alias, out handler!);
  }

  private static string ResolveAlias(string? alias, Type type)
  {
    return string.IsNullOrWhiteSpace(alias) ? type.Name : alias;
  }

  private static string RequireAlias(string alias)
  {
    if (string.IsNullOrWhiteSpace(alias))
    {
      throw new ArgumentException("Alias darf nicht leer sein.", nameof(alias));
    }

    return alias;
  }

  private static void Add<TValue>(Dictionary<string, TValue> target, string alias, TValue value, string kind)
  {
    if (target.ContainsKey(alias))
    {
      throw new InvalidOperationException($"{kind}-Alias '{alias}' ist bereits registriert.");
    }

    target.Add(alias, value);
  }
}
