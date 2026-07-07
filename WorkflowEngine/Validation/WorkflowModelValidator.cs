using WorkflowEngine.Catalog;
using WorkflowEngine.Configuration;

namespace WorkflowEngine.Validation;

/// <summary>
/// Statische Analyse eines <see cref="WorkflowConfigurationModel"/> gegen den Baustein-Katalog:
/// tote Zielnamen, unbekannte Aliase, unverdrahtete Outcomes und fehlender Pflicht-Kontext.
/// Deckt keine Hook-Aliase (Before/After/OnException) ab, da diese nicht im Katalog stehen —
/// dafür bleibt der abschließende Aufruf von <see cref="WorkflowDefinitionFactory.Create"/> mit
/// der vollständigen Registry die letzte, verbindliche Prüfung vor dem Speichern.
/// </summary>
public sealed class WorkflowModelValidator
{
  public IReadOnlyList<ValidationIssue> Validate(WorkflowConfigurationModel model, IBlockCatalog catalog)
  {
    if (model is null)
    {
      throw new ArgumentNullException(nameof(model));
    }

    if (catalog is null)
    {
      throw new ArgumentNullException(nameof(catalog));
    }

    var issues = new List<ValidationIssue>();
    var steps = BuildStepLookup(model, issues);

    ValidateStartStep(model, steps, issues);
    ValidateTargetsExist(model, steps, issues);
    ValidateAliasesInCatalog(model, catalog, issues);
    ValidateOutcomesWired(model, catalog, issues);
    ValidateDataFlow(model, steps, catalog, issues);
    ValidateReachabilityAndDeadEnds(model, steps, issues);

    return issues;
  }

  private static Dictionary<string, WorkflowStepConfigurationModel> BuildStepLookup(
    WorkflowConfigurationModel model,
    List<ValidationIssue> issues)
  {
    var lookup = new Dictionary<string, WorkflowStepConfigurationModel>(StringComparer.Ordinal);

    foreach (var step in model.Steps)
    {
      if (string.IsNullOrWhiteSpace(step.Name))
      {
        issues.Add(new ValidationIssue(ValidationSeverity.Error, null, "Ein Step hat keinen Namen."));
        continue;
      }

      if (!lookup.TryAdd(step.Name, step))
      {
        issues.Add(new ValidationIssue(ValidationSeverity.Error, step.Name,
          $"Step-Name '{step.Name}' ist mehrfach definiert."));
      }
    }

    return lookup;
  }

  private static void ValidateStartStep(
    WorkflowConfigurationModel model,
    IReadOnlyDictionary<string, WorkflowStepConfigurationModel> steps,
    List<ValidationIssue> issues)
  {
    if (string.IsNullOrWhiteSpace(model.StartStep) || !steps.ContainsKey(model.StartStep))
    {
      issues.Add(new ValidationIssue(ValidationSeverity.Error, null,
        $"StartStep '{model.StartStep}' ist nicht als Step definiert."));
    }
  }

  private static void ValidateTargetsExist(
    WorkflowConfigurationModel model,
    IReadOnlyDictionary<string, WorkflowStepConfigurationModel> steps,
    List<ValidationIssue> issues)
  {
    foreach (var step in model.Steps)
    {
      foreach (var (outcome, target) in step.Transitions)
      {
        if (!steps.ContainsKey(target))
        {
          issues.Add(new ValidationIssue(ValidationSeverity.Error, step.Name,
            $"Transition '{outcome}' verweist auf unbekannten Step '{target}'."));
        }
      }

      if (!string.IsNullOrWhiteSpace(step.Next) && !steps.ContainsKey(step.Next))
      {
        issues.Add(new ValidationIssue(ValidationSeverity.Error, step.Name,
          $"Next verweist auf unbekannten Step '{step.Next}'."));
      }

      if (!string.IsNullOrWhiteSpace(step.OnExceptionNextStep) && !steps.ContainsKey(step.OnExceptionNextStep))
      {
        issues.Add(new ValidationIssue(ValidationSeverity.Error, step.Name,
          $"OnExceptionNextStep verweist auf unbekannten Step '{step.OnExceptionNextStep}'."));
      }
    }
  }

  private static void ValidateAliasesInCatalog(
    WorkflowConfigurationModel model,
    IBlockCatalog catalog,
    List<ValidationIssue> issues)
  {
    foreach (var step in model.Steps)
    {
      if (!string.IsNullOrWhiteSpace(step.Execute))
      {
        if (!catalog.TryGet(step.Execute, out var descriptor))
        {
          issues.Add(new ValidationIssue(ValidationSeverity.Error, step.Name,
            $"Execute-Alias '{step.Execute}' ist nicht im Baustein-Katalog."));
        }
        else if (descriptor.Kind != BlockKind.Action)
        {
          issues.Add(new ValidationIssue(ValidationSeverity.Error, step.Name,
            $"Execute-Alias '{step.Execute}' referenziert keinen Action-Baustein."));
        }
      }

      if (!string.IsNullOrWhiteSpace(step.Component))
      {
        if (!catalog.TryGet(step.Component, out var descriptor))
        {
          issues.Add(new ValidationIssue(ValidationSeverity.Error, step.Name,
            $"Component-Alias '{step.Component}' ist nicht im Baustein-Katalog."));
        }
        else if (descriptor.Kind != BlockKind.Page)
        {
          issues.Add(new ValidationIssue(ValidationSeverity.Error, step.Name,
            $"Component-Alias '{step.Component}' referenziert keinen Page-Baustein."));
        }
      }
    }
  }

  /// <summary>
  /// Ermittelt, welcher Baustein den tatsächlichen Ausgang eines Steps bestimmt: Ist ein
  /// Execute-Step gesetzt, entscheidet dessen StepResult (auch wenn zusätzlich eine Component
  /// gesetzt ist, z.B. "LoadCarrierBooking"). Sonst die Component selbst (Pages steuern ihre
  /// Navigation aktiv über AdvanceViaAsync).
  /// </summary>
  private static bool TryGetOutcomeSourceBlock(
    WorkflowStepConfigurationModel step,
    IBlockCatalog catalog,
    out BlockDescriptor descriptor)
  {
    if (!string.IsNullOrWhiteSpace(step.Execute) && catalog.TryGet(step.Execute, out descriptor!))
    {
      return true;
    }

    if (!string.IsNullOrWhiteSpace(step.Component) && catalog.TryGet(step.Component, out descriptor!))
    {
      return true;
    }

    descriptor = null!;
    return false;
  }

  private static void ValidateOutcomesWired(
    WorkflowConfigurationModel model,
    IBlockCatalog catalog,
    List<ValidationIssue> issues)
  {
    foreach (var step in model.Steps)
    {
      if (!TryGetOutcomeSourceBlock(step, catalog, out var descriptor))
      {
        continue;
      }

      // Der synthetische "Default"-Ausgang (Page ohne [Outcome]-Deklaration) steht für eine
      // unbedingte Fortsetzung über "next" und muss nicht per Transition verdrahtet werden.
      var declaredOutcomes = descriptor.Outcomes.Where(o => !o.IsImplicit).ToList();

      foreach (var outcome in declaredOutcomes)
      {
        if (!step.Transitions.ContainsKey(outcome.Name))
        {
          issues.Add(new ValidationIssue(ValidationSeverity.Error, step.Name,
            $"Outcome '{outcome.Name}' des Bausteins '{descriptor.Alias}' ist nicht verdrahtet (transitions)."));
        }
      }

      var declaredNames = declaredOutcomes.Select(o => o.Name).ToHashSet(StringComparer.Ordinal);
      foreach (var outcome in step.Transitions.Keys)
      {
        if (!declaredNames.Contains(outcome))
        {
          issues.Add(new ValidationIssue(ValidationSeverity.Error, step.Name,
            $"Transition-Outcome '{outcome}' ist für Baustein '{descriptor.Alias}' nicht deklariert."));
        }
      }
    }
  }

  /// <summary>
  /// Fixpunkt-Iteration über den Graphen (Transitions/Next als Kanten): Für jeden Step wird die
  /// Schnittmenge der auf ALLEN eingehenden Pfaden garantiert vorhandenen Kontext-Schlüssel
  /// berechnet, beginnend leer beim StartStep. Component-Outputs eines Steps stehen bereits dem
  /// gefusionierten Execute desselben Steps zur Verfügung (Page sammelt Daten, dann läuft die
  /// Action, siehe z.B. "CreateUser"); Execute-Outputs stehen erst nachfolgenden Steps zur
  /// Verfügung.
  /// </summary>
  private static void ValidateDataFlow(
    WorkflowConfigurationModel model,
    IReadOnlyDictionary<string, WorkflowStepConfigurationModel> steps,
    IBlockCatalog catalog,
    List<ValidationIssue> issues)
  {
    if (string.IsNullOrWhiteSpace(model.StartStep) || !steps.ContainsKey(model.StartStep))
    {
      return; // Bereits als Fehler gemeldet; ohne validen Start lässt sich kein Datenfluss berechnen.
    }

    var available = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal)
    {
      [model.StartStep] = new HashSet<string>(StringComparer.Ordinal)
    };

    var worklist = new Queue<string>();
    worklist.Enqueue(model.StartStep);

    while (worklist.Count > 0)
    {
      var stepName = worklist.Dequeue();
      if (!steps.TryGetValue(stepName, out var step))
      {
        continue;
      }

      var afterComponent = new HashSet<string>(available[stepName], StringComparer.Ordinal);
      afterComponent.UnionWith(GetOutputs(step.Component, catalog));

      var produced = new HashSet<string>(afterComponent, StringComparer.Ordinal);
      produced.UnionWith(GetOutputs(step.Execute, catalog));

      foreach (var target in GetSuccessors(step))
      {
        if (!steps.ContainsKey(target))
        {
          continue; // Toter Zielname, bereits separat gemeldet.
        }

        if (!available.TryGetValue(target, out var existing))
        {
          available[target] = new HashSet<string>(produced, StringComparer.Ordinal);
          worklist.Enqueue(target);
          continue;
        }

        var intersected = new HashSet<string>(existing, StringComparer.Ordinal);
        intersected.IntersectWith(produced);
        if (intersected.Count != existing.Count)
        {
          available[target] = intersected;
          worklist.Enqueue(target);
        }
      }
    }

    foreach (var step in model.Steps)
    {
      if (!available.TryGetValue(step.Name, out var incoming))
      {
        continue; // Nicht erreichbar, separat als Warnung gemeldet.
      }

      foreach (var input in GetRequiredInputs(step.Component, catalog))
      {
        if (!incoming.Contains(input))
        {
          issues.Add(new ValidationIssue(ValidationSeverity.Error, step.Name,
            $"Pflicht-Kontext '{input}' wird nicht auf allen Pfaden vom StartStep produziert."));
        }
      }

      var afterComponent = new HashSet<string>(incoming, StringComparer.Ordinal);
      afterComponent.UnionWith(GetOutputs(step.Component, catalog));

      foreach (var input in GetRequiredInputs(step.Execute, catalog))
      {
        if (!afterComponent.Contains(input))
        {
          issues.Add(new ValidationIssue(ValidationSeverity.Error, step.Name,
            $"Pflicht-Kontext '{input}' wird nicht auf allen Pfaden vom StartStep produziert."));
        }
      }
    }
  }

  private static IEnumerable<string> GetOutputs(string? alias, IBlockCatalog catalog)
  {
    if (string.IsNullOrWhiteSpace(alias) || !catalog.TryGet(alias, out var descriptor))
    {
      return [];
    }

    return descriptor.Outputs.Select(o => o.Key);
  }

  private static IEnumerable<string> GetRequiredInputs(string? alias, IBlockCatalog catalog)
  {
    if (string.IsNullOrWhiteSpace(alias) || !catalog.TryGet(alias, out var descriptor))
    {
      return [];
    }

    return descriptor.Inputs.Where(i => i.Required).Select(i => i.Key);
  }

  private static IEnumerable<string> GetSuccessors(WorkflowStepConfigurationModel step)
  {
    foreach (var target in step.Transitions.Values)
    {
      yield return target;
    }

    if (!string.IsNullOrWhiteSpace(step.Next))
    {
      yield return step.Next;
    }
  }

  private static void ValidateReachabilityAndDeadEnds(
    WorkflowConfigurationModel model,
    IReadOnlyDictionary<string, WorkflowStepConfigurationModel> steps,
    List<ValidationIssue> issues)
  {
    if (!string.IsNullOrWhiteSpace(model.StartStep) && steps.ContainsKey(model.StartStep))
    {
      var reachable = new HashSet<string>(StringComparer.Ordinal) { model.StartStep };
      var queue = new Queue<string>();
      queue.Enqueue(model.StartStep);

      while (queue.Count > 0)
      {
        var stepName = queue.Dequeue();
        if (!steps.TryGetValue(stepName, out var step))
        {
          continue;
        }

        var successors = GetSuccessors(step).ToList();
        if (!string.IsNullOrWhiteSpace(step.OnExceptionNextStep))
        {
          successors.Add(step.OnExceptionNextStep);
        }

        foreach (var target in successors)
        {
          if (steps.ContainsKey(target) && reachable.Add(target))
          {
            queue.Enqueue(target);
          }
        }
      }

      foreach (var step in model.Steps)
      {
        if (!string.IsNullOrWhiteSpace(step.Name) && !reachable.Contains(step.Name))
        {
          issues.Add(new ValidationIssue(ValidationSeverity.Warning, step.Name,
            $"Step '{step.Name}' ist vom StartStep aus nicht erreichbar."));
        }
      }
    }

    foreach (var step in model.Steps)
    {
      if (step.Transitions.Count == 0 && string.IsNullOrWhiteSpace(step.Next))
      {
        issues.Add(new ValidationIssue(ValidationSeverity.Warning, step.Name,
          $"Step '{step.Name}' hat keine ausgehende Kante (ggf. bewusstes Workflow-Ende)."));
      }
    }
  }
}
