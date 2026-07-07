namespace WorkflowEngine.Catalog;

/// <summary>Ein benannter Ausgang eines Bausteins, an dem der Editor eine Kante anhängen kann.</summary>
public sealed record OutcomePort(string Name, string DisplayName);
