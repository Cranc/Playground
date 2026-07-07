using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using WorkflowEngine.Catalog;

namespace WorkflowEngine.DependencyInjection;

public static class ServiceCollectionExtensions
{
  /// <summary>
  /// Registriert die Workflow-Engine-Dienste. Werden Assemblies angegeben, wird zusätzlich
  /// ein <see cref="IBlockCatalog"/> per Reflection-Scan gebaut und als Singleton registriert.
  /// </summary>
  public static IServiceCollection AddWorkflowEngine(this IServiceCollection services, params Assembly[] blockAssemblies)
  {
    if (blockAssemblies is { Length: > 0 })
    {
      var catalog = new BlockCatalogBuilder().Build(blockAssemblies);
      services.AddSingleton(catalog);
    }

    return services;
  }
}
