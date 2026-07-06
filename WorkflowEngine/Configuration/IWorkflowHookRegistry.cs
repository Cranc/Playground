namespace WorkflowEngine.Configuration;

/// <summary>
/// Löst die in einer Konfiguration verwendeten Hook-Aliase auf die konkreten Delegates auf.
/// Damit bleibt beliebige Logik (z.B. Kontext-Initialisierung, Fehlerbehandlung) im Code der
/// Anwendung, während die Konfiguration nur den Namen des Hooks referenziert.
/// </summary>
public interface IWorkflowHookRegistry
{
  /// <summary>Löst den Alias auf eine Before-/After-Aktion auf.</summary>
  bool TryResolveAction(string alias, out Func<WorkflowContext, Task> action);

  /// <summary>Löst den Alias auf einen Exception-Handler auf, der optional den nächsten Step liefert.</summary>
  bool TryResolveExceptionHandler(string alias, out Func<WorkflowContext, Exception, Task<string?>> handler);
}

/// <summary>
/// Bündelt Typ- und Hook-Auflösung, sodass Factory und Loader eine einzelne Registry
/// entgegennehmen können.
/// </summary>
public interface IWorkflowRegistry : IWorkflowTypeRegistry, IWorkflowHookRegistry
{
}
