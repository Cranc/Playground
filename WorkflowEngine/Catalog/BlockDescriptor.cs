namespace WorkflowEngine.Catalog;

/// <summary>Metadaten eines im Editor darstellbaren Bausteins, gewonnen aus Attributen per Assembly-Scan.</summary>
public sealed record BlockDescriptor(
  string Alias,
  BlockKind Kind,
  string DisplayName,
  string Category,
  string? Description,
  Type ImplementationType,
  IReadOnlyList<ContextPort> Inputs,
  IReadOnlyList<ContextPort> Outputs,
  IReadOnlyList<OutcomePort> Outcomes);
