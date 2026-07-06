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
  /// <summary>Erzeugt eine <see cref="WorkflowDefinition"/> aus Modell und Typ-Registry.</summary>
  public WorkflowDefinition Create(WorkflowConfigurationModel model, IWorkflowTypeRegistry registry)
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

      if (!string.IsNullOrWhiteSpace(stepModel.Execute))
      {
        if (!registry.TryResolveStep(stepModel.Execute, out var stepType))
        {
          throw new InvalidOperationException(
            $"Step '{stepModel.Name}': Execute-Alias '{stepModel.Execute}' ist nicht in der Typ-Registry registriert.");
        }

        step.Execute(stepType);
      }

      if (!string.IsNullOrWhiteSpace(stepModel.Component))
      {
        if (!registry.TryResolveComponent(stepModel.Component, out var componentType))
        {
          throw new InvalidOperationException(
            $"Step '{stepModel.Name}': Component-Alias '{stepModel.Component}' ist nicht in der Typ-Registry registriert.");
        }

        step.Component(componentType);
      }

      if (stepModel.Retry > 0)
      {
        step.Retry(stepModel.Retry);
      }

      if (!string.IsNullOrWhiteSpace(stepModel.OnExceptionNextStep))
      {
        var nextStep = stepModel.OnExceptionNextStep;
        step.OnException((_, _) => nextStep);
      }
    }

    return builder.Build();
  }
}
