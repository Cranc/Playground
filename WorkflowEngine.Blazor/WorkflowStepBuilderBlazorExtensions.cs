using Microsoft.AspNetCore.Components;
using WorkflowEngine.Definition;

namespace WorkflowEngine.Blazor;

public static class WorkflowStepBuilderBlazorExtensions
{
  public static WorkflowStepBuilder Component<TComponent>(this WorkflowStepBuilder builder)
    where TComponent : IComponent
  {
    return builder.WithComponent(typeof(TComponent));
  }
}
