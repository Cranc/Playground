using WorkflowEngine.Catalog;

namespace WorkflowEngine.Tests.Fixtures;

/// <summary>Test-Bausteine für <see cref="WorkflowModelValidatorTests"/>: bewusst minimal, ohne echtes Verhalten.</summary>
[WorkflowBlock(Category = "Test")]
[ProducesContext("Foo", typeof(string))]
public sealed class ProducesFooStep : IWorkflowStep
{
  public Task<StepResult> ExecuteAsync(WorkflowContext context) => Task.FromResult(StepResult.Ok());
}

[WorkflowBlock(Category = "Test")]
[ConsumesContext("Foo", typeof(string))]
public sealed class RequiresFooStep : IWorkflowStep
{
  public Task<StepResult> ExecuteAsync(WorkflowContext context) => Task.FromResult(StepResult.Ok());
}
