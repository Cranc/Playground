namespace WorkflowEngine;

/// <summary>
/// Markerinterface für Page-Bausteine (Blazor-Komponenten, die einen Workflow-Step darstellen).
/// Wird von <c>WorkflowStepComponent</c> in <c>WorkflowEngine.Blazor</c> implementiert, damit der
/// Katalog-Scan in <c>WorkflowEngine</c> Page-Bausteine erkennen kann, ohne selbst einen Verweis
/// auf das Blazor-Projekt zu benötigen.
/// </summary>
public interface IWorkflowPageComponent
{
}
