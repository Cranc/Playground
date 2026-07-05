using Microsoft.Extensions.DependencyInjection;

namespace WorkflowEngine.DependencyInjection;

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddWorkflowEngine(this IServiceCollection services)
  {
    return services;
  }
}
