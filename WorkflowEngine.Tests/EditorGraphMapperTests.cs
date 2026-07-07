using WorkflowEngine.Configuration;
using WorkflowEngine.Editor.Model;
using Work.Workflows.DriverTour;
using Xunit;

namespace WorkflowEngine.Tests;

public class EditorGraphMapperTests
{
  private static string RepositoryRoot =>
    Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));

  private static WorkflowConfigurationModel ReadDriverTourModel()
  {
    var jsonPath = Path.Combine(RepositoryRoot, "Work", DriverTourWorkflowConfig.ConfigFileRelativePath);
    return new JsonWorkflowConfigurationReader().Read(File.ReadAllText(jsonPath));
  }

  [Fact]
  public void ToEditorGraph_CreatesOneNodePerStepAndOneEdgePerTransitionOrNext()
  {
    var model = ReadDriverTourModel();

    var graph = EditorGraphMapper.ToEditorGraph(model, layout: null);

    Assert.Equal(model.Name, graph.Name);
    Assert.Equal(model.StartStep, graph.StartNodeId);
    Assert.Equal(model.Steps.Count, graph.Nodes.Count);

    var expectedEdgeCount = model.Steps.Sum(s => s.Transitions.Count + (string.IsNullOrWhiteSpace(s.Next) ? 0 : 1));
    Assert.Equal(expectedEdgeCount, graph.Edges.Count);

    var loadCarrierBooking = Assert.Single(graph.Nodes, n => n.Id == "LoadCarrierBooking");
    Assert.Equal("LoadCarrierBookingPage", loadCarrierBooking.ComponentAlias);
    Assert.Equal("AdvanceStopStep", loadCarrierBooking.ExecuteAlias);

    Assert.Contains(graph.Edges, e => e is { FromNodeId: "LoadCarrierBooking", Outcome: "StopArrival", ToNodeId: "StopArrival" });
    Assert.Contains(graph.Edges, e => e.FromNodeId == "TourStart" && e.IsUnconditional && e.ToNodeId == "StopArrival");
  }

  [Fact]
  public void RoundTrip_ModelThroughEditorGraph_IsLossless()
  {
    var original = ReadDriverTourModel();

    var graph = EditorGraphMapper.ToEditorGraph(original, layout: null);
    var (roundTripped, _) = EditorGraphMapper.ToConfiguration(graph);

    Assert.Equal(original.Name, roundTripped.Name);
    Assert.Equal(original.StartStep, roundTripped.StartStep);
    Assert.Equal(original.Steps.Count, roundTripped.Steps.Count);

    foreach (var expected in original.Steps)
    {
      var actual = Assert.Single(roundTripped.Steps, s => s.Name == expected.Name);
      Assert.Equal(expected.Before, actual.Before);
      Assert.Equal(expected.Execute, actual.Execute);
      Assert.Equal(expected.Component, actual.Component);
      Assert.Equal(expected.After, actual.After);
      Assert.Equal(expected.Retry, actual.Retry);
      Assert.Equal(expected.OnException, actual.OnException);
      Assert.Equal(expected.OnExceptionNextStep, actual.OnExceptionNextStep);
      Assert.Equal(expected.Next, actual.Next);
      Assert.Equal(expected.Transitions, actual.Transitions);
    }
  }

  [Fact]
  public void ToConfiguration_ProducesLayoutMatchingNodePositions()
  {
    var graph = new EditorGraph
    {
      Name = "Test",
      StartNodeId = "A",
      Nodes =
      {
        new EditorNode { Id = "A", ExecuteAlias = "SomeStep", X = 12, Y = 34 }
      }
    };

    var (_, layout) = EditorGraphMapper.ToConfiguration(graph);

    var nodeLayout = Assert.Single(layout.Nodes);
    Assert.Equal("A", nodeLayout.StepName);
    Assert.Equal(12, nodeLayout.X);
    Assert.Equal(34, nodeLayout.Y);
  }
}
