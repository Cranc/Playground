namespace WorkflowEngine.Editor.Components.Editor;

/// <summary>Geometrie-Konstanten, gemeinsam genutzt von <c>BlockNode</c> und <c>ConnectionLayer</c>,
/// damit Kanten exakt an den Ports andocken.</summary>
public static class NodeMetrics
{
  public const double Width = 200;
  public const double HeaderHeight = 42;
  public const double PortRowHeight = 18;
  public const double PortsTopPadding = 10;

  public static double GetHeight(int inputCount, int outcomeCount)
  {
    var rows = Math.Max(Math.Max(inputCount, outcomeCount), 1);
    return HeaderHeight + PortsTopPadding + (rows * PortRowHeight) + PortsTopPadding;
  }

  public static double GetInputPortY(int index) => HeaderHeight + PortsTopPadding + (index * PortRowHeight) + (PortRowHeight / 2);

  public static double GetOutcomePortY(int index) => HeaderHeight + PortsTopPadding + (index * PortRowHeight) + (PortRowHeight / 2);
}
