namespace WorkflowEngine.Catalog;

/// <summary>Ein per <see cref="ConsumesContextAttribute"/>/<see cref="ProducesContextAttribute"/> deklarierter Kontext-Schlüssel.</summary>
public sealed record ContextPort(string Key, Type Type, bool Required);
