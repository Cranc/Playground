namespace WorkflowEngine.Editor.Layout;

/// <summary>Position eines Bausteins auf der Editor-Canvas.</summary>
public sealed record NodeLayout(string StepName, double X, double Y);

/// <summary>
/// Editor-seitige Zusatzdaten (Node-Positionen) zu einem Workflow. Bewusst getrennt von der
/// Laufzeit-Konfiguration (<c>{Name}.json</c>) gehalten und als Sidecar-Datei
/// (<c>{Name}.layout.json</c>) gespeichert, damit die Runtime-Config sauber bleibt.
/// </summary>
public sealed record WorkflowEditorLayout(string WorkflowName, List<NodeLayout> Nodes);
