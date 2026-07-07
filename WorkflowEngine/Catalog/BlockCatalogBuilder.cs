using System.Reflection;

namespace WorkflowEngine.Catalog;

/// <summary>
/// Baut einen <see cref="IBlockCatalog"/> per Reflection-Scan über die übergebenen Assemblies.
/// Erfasst werden alle konkreten Typen, die <see cref="IWorkflowStep"/> implementieren oder
/// <see cref="IWorkflowPageComponent"/> (z.B. <c>WorkflowStepComponent</c>) sind, sowie zusätzlich
/// alle mit <see cref="WorkflowBlockAttribute"/> markierten Typen.
/// </summary>
public sealed class BlockCatalogBuilder
{
  public IBlockCatalog Build(params Assembly[] assemblies)
  {
    if (assemblies is null || assemblies.Length == 0)
    {
      throw new ArgumentException("Mindestens eine Assembly muss angegeben werden.", nameof(assemblies));
    }

    var descriptors = new Dictionary<string, BlockDescriptor>(StringComparer.Ordinal);

    foreach (var type in assemblies.Distinct().SelectMany(GetLoadableTypes))
    {
      if (!IsCandidate(type))
      {
        continue;
      }

      var descriptor = Describe(type);

      if (descriptors.TryGetValue(descriptor.Alias, out var existing))
      {
        throw new InvalidOperationException(
          $"Baustein-Alias '{descriptor.Alias}' ist bereits für '{existing.ImplementationType.FullName}' vergeben " +
          $"(Konflikt mit '{descriptor.ImplementationType.FullName}').");
      }

      descriptors.Add(descriptor.Alias, descriptor);
    }

    return new BlockCatalog(descriptors.Values.ToList());
  }

  /// <summary>
  /// Wie <see cref="Assembly.GetTypes"/>, aber tolerant gegenüber nicht ladbaren Typen (z.B. wenn
  /// die scannende Anwendung nicht alle Abhängigkeiten der gescannten Assembly referenziert).
  /// </summary>
  private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
  {
    try
    {
      return assembly.GetTypes();
    }
    catch (ReflectionTypeLoadException ex)
    {
      return ex.Types.Where(t => t is not null)!;
    }
  }

  private static bool IsCandidate(Type type)
  {
    if (!type.IsClass || type.IsAbstract || type.ContainsGenericParameters)
    {
      return false;
    }

    return type.IsDefined(typeof(WorkflowBlockAttribute))
      || typeof(IWorkflowStep).IsAssignableFrom(type)
      || typeof(IWorkflowPageComponent).IsAssignableFrom(type);
  }

  private static BlockDescriptor Describe(Type type)
  {
    var attribute = type.GetCustomAttribute<WorkflowBlockAttribute>();
    var kind = DetermineKind(type);

    var alias = string.IsNullOrWhiteSpace(attribute?.Alias) ? type.Name : attribute.Alias;
    var displayName = string.IsNullOrWhiteSpace(attribute?.DisplayName) ? type.Name : attribute.DisplayName;
    var category = attribute?.Category ?? "Allgemein";

    var inputs = type.GetCustomAttributes<ConsumesContextAttribute>()
      .Select(a => new ContextPort(a.Key, a.Type, a.Required))
      .ToList();

    var outputs = type.GetCustomAttributes<ProducesContextAttribute>()
      .Select(a => new ContextPort(a.Key, a.Type, Required: true))
      .ToList();

    var outcomes = type.GetCustomAttributes<OutcomeAttribute>()
      .Select(a => new OutcomePort(a.Name, string.IsNullOrWhiteSpace(a.DisplayName) ? a.Name : a.DisplayName))
      .ToList();

    if (outcomes.Count == 0 && kind == BlockKind.Page)
    {
      outcomes.Add(new OutcomePort("Default", "Weiter", IsImplicit: true));
    }

    return new BlockDescriptor(alias, kind, displayName, category, attribute?.Description, type, inputs, outputs, outcomes);
  }

  private static BlockKind DetermineKind(Type type)
  {
    if (typeof(IWorkflowStep).IsAssignableFrom(type))
    {
      return BlockKind.Action;
    }

    if (typeof(IWorkflowPageComponent).IsAssignableFrom(type))
    {
      return BlockKind.Page;
    }

    throw new InvalidOperationException(
      $"Typ '{type.FullName}' ist mit [{nameof(WorkflowBlockAttribute)}] markiert, implementiert aber weder " +
      $"'{nameof(IWorkflowStep)}' noch '{nameof(IWorkflowPageComponent)}'.");
  }

  private sealed class BlockCatalog : IBlockCatalog
  {
    private readonly Dictionary<string, BlockDescriptor> _byAlias;

    public BlockCatalog(IReadOnlyList<BlockDescriptor> blocks)
    {
      Blocks = blocks;
      _byAlias = blocks.ToDictionary(b => b.Alias, StringComparer.Ordinal);
    }

    public IReadOnlyList<BlockDescriptor> Blocks { get; }

    public bool TryGet(string alias, out BlockDescriptor descriptor)
    {
      return _byAlias.TryGetValue(alias, out descriptor!);
    }
  }
}
