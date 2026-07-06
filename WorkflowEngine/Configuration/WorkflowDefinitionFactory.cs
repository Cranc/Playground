using WorkflowEngine.Definition;

namespace WorkflowEngine.Configuration;

/// <summary>
/// Übersetzt ein <see cref="WorkflowConfigurationModel"/> in eine ausführbare
/// <see cref="WorkflowDefinition"/>. Die eigentliche Konstruktion und Validierung erfolgt
/// über den bestehenden <see cref="WorkflowBuilder"/>, sodass es nur eine Quelle der
/// Wahrheit für die Regeln gibt.
/// </summary>
public sealed class WorkflowDefinitionFactory
{
  /// <summary>Erzeugt eine <see cref="WorkflowDefinition"/> aus Modell und Registry.</summary>
  public WorkflowDefinition Create(WorkflowConfigurationModel model, IWorkflowRegistry registry)
  {
    if (model is null)
    {
      throw new ArgumentNullException(nameof(model));
    }

    if (registry is null)
    {
      throw new ArgumentNullException(nameof(registry));
    }

    var builder = new WorkflowBuilder(model.Name).StartWith(model.StartStep);

    foreach (var stepModel in model.Steps)
    {
      var step = builder.Step(stepModel.Name);

      if (!string.IsNullOrWhiteSpace(stepModel.Before))
      {
        if (!registry.TryResolveAction(stepModel.Before, out var before))
        {
          throw new InvalidOperationException(
            $"Step '{stepModel.Name}': Before-Alias '{stepModel.Before}' ist nicht in der Registry registriert.");
        }

        step.Before(before);
      }

      if (!string.IsNullOrWhiteSpace(stepModel.Execute))
      {
        if (!registry.TryResolveStep(stepModel.Execute, out var stepType))
        {
          throw new InvalidOperationException(
            $"Step '{stepModel.Name}': Execute-Alias '{stepModel.Execute}' ist nicht in der Registry registriert.");
        }

        step.Execute(stepType);
      }

      if (!string.IsNullOrWhiteSpace(stepModel.Component))
      {
        if (!registry.TryResolveComponent(stepModel.Component, out var componentType))
        {
          throw new InvalidOperationException(
            $"Step '{stepModel.Name}': Component-Alias '{stepModel.Component}' ist nicht in der Registry registriert.");
        }

        step.Component(componentType);
      }

      if (!string.IsNullOrWhiteSpace(stepModel.After))
      {
        if (!registry.TryResolveAction(stepModel.After, out var after))
        {
          throw new InvalidOperationException(
            $"Step '{stepModel.Name}': After-Alias '{stepModel.After}' ist nicht in der Registry registriert.");
        }

        step.After(after);
      }

      if (stepModel.Retry > 0)
      {
        step.Retry(stepModel.Retry);
      }

      ConfigureException(step, stepModel, registry);
    }

    return builder.Build();
  }

  private static void ConfigureException(
    WorkflowStepBuilder step,
    WorkflowStepConfigurationModel stepModel,
    IWorkflowRegistry registry)
  {
    if (!string.IsNullOrWhiteSpace(stepModel.OnException))
    {
      if (!registry.TryResolveExceptionHandler(stepModel.OnException, out var handler))
      {
        throw new InvalidOperationException(
          $"Step '{stepModel.Name}': OnException-Alias '{stepModel.OnException}' ist nicht in der Registry registriert.");
      }

      step.OnException(handler);
      return;
    }

    if (!string.IsNullOrWhiteSpace(stepModel.OnExceptionNextStep))
    {
      var nextStep = stepModel.OnExceptionNextStep;
      step.OnException((_, _) => nextStep);
    }
  }
}
