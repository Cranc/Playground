namespace WorkflowEngine.Catalog;

/// <summary>Ein benannter Ausgang eines Bausteins, an dem der Editor eine Kante anhängen kann.</summary>
/// <param name="IsImplicit">
/// <c>true</c> für den synthetischen "Default"-Ausgang, den der Katalog Page-Bausteinen ohne
/// eigene <see cref="OutcomeAttribute"/>-Deklaration automatisch zuweist. Solche Ausgänge stehen
/// für eine unbedingte Fortsetzung (<c>Next</c>) und müssen nicht per Transition verdrahtet werden.
/// </param>
public sealed record OutcomePort(string Name, string DisplayName, bool IsImplicit = false);
