namespace WorkflowEngine.Validation;

/// <summary>Ein Befund des <see cref="WorkflowModelValidator"/>. <paramref name="StepName"/> ist
/// <c>null</c>, wenn sich der Befund auf den Workflow als Ganzes bezieht (z.B. StartStep).</summary>
public sealed record ValidationIssue(ValidationSeverity Severity, string? StepName, string Message);
