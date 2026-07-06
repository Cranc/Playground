namespace WorkflowEngine.Configuration;

/// <summary>
/// Löst die in einer Konfiguration verwendeten Aliase auf die konkreten .NET-Typen auf.
/// Dadurch enthält die Konfiguration nur kurze, stabile Namen statt assembly-qualifizierter
/// Typnamen, und es wird kein beliebiger Typ per Reflection geladen.
/// </summary>
public interface IWorkflowTypeRegistry
{
  /// <summary>Versucht, den Alias auf einen registrierten <see cref="IWorkflowStep"/>-Typ aufzulösen.</summary>
  bool TryResolveStep(string alias, out Type stepType);

  /// <summary>Versucht, den Alias auf einen registrierten Component-Typ aufzulösen.</summary>
  bool TryResolveComponent(string alias, out Type componentType);
}
