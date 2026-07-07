using WorkflowEngine.Configuration;
using Work.Workflows.DriverTour;
using Work.Workflows;
using Xunit;

namespace WorkflowEngine.Tests;

public class JsonWorkflowConfigurationWriterTests
{
  private static string RepositoryRoot =>
    Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));

  [Fact]
  public void RoundTrip_DriverTourWorkflowJson_IsLosslessAfterWriteAndRead()
  {
    AssertRoundTripIsLossless(DriverTourWorkflowConfig.ConfigFileRelativePath);
  }

  [Fact]
  public void RoundTrip_UserWizardWorkflowJson_IsLosslessAfterWriteAndRead()
  {
    AssertRoundTripIsLossless(UserWizardWorkflowConfig.ConfigFileRelativePath);
  }

  private static void AssertRoundTripIsLossless(string configFileRelativePath)
  {
    var jsonPath = Path.Combine(RepositoryRoot, "Work", configFileRelativePath);
    var reader = new JsonWorkflowConfigurationReader();
    var writer = new JsonWorkflowConfigurationWriter();

    var original = reader.Read(File.ReadAllText(jsonPath));
    var roundTripped = reader.Read(writer.Write(original));

    AssertModelsEqual(original, roundTripped);
  }

  [Fact]
  public void Write_OmitsEmptyTransitionsNextAndUnsetOptionalFields()
  {
    var model = new WorkflowConfigurationModel
    {
      Name = "Minimal",
      StartStep = "A",
      Steps = { new WorkflowStepConfigurationModel { Name = "A", Execute = "SomeStep" } }
    };

    var json = new JsonWorkflowConfigurationWriter().Write(model);

    Assert.DoesNotContain("transitions", json);
    Assert.DoesNotContain("next", json, StringComparison.OrdinalIgnoreCase);
    Assert.DoesNotContain("retry", json, StringComparison.OrdinalIgnoreCase);
    Assert.DoesNotContain("before", json);
    Assert.DoesNotContain("component", json);
    Assert.DoesNotContain("after", json);
  }

  [Fact]
  public void Write_IncludesTransitionsAndNextWhenSet()
  {
    var model = new WorkflowConfigurationModel
    {
      Name = "WithTransitions",
      StartStep = "A",
      Steps =
      {
        new WorkflowStepConfigurationModel
        {
          Name = "A",
          Execute = "SomeStep",
          Transitions = { ["Foo"] = "B" },
          Next = "B"
        },
        new WorkflowStepConfigurationModel { Name = "B", Execute = "OtherStep" }
      }
    };

    var writer = new JsonWorkflowConfigurationWriter();
    var json = writer.Write(model);
    var roundTripped = new JsonWorkflowConfigurationReader().Read(json);

    AssertModelsEqual(model, roundTripped);
  }

  private static void AssertModelsEqual(WorkflowConfigurationModel expected, WorkflowConfigurationModel actual)
  {
    Assert.Equal(expected.Name, actual.Name);
    Assert.Equal(expected.StartStep, actual.StartStep);
    Assert.Equal(expected.Steps.Count, actual.Steps.Count);

    for (var i = 0; i < expected.Steps.Count; i++)
    {
      var e = expected.Steps[i];
      var a = actual.Steps[i];

      Assert.Equal(e.Name, a.Name);
      Assert.Equal(e.Before, a.Before);
      Assert.Equal(e.Execute, a.Execute);
      Assert.Equal(e.Component, a.Component);
      Assert.Equal(e.After, a.After);
      Assert.Equal(e.Retry, a.Retry);
      Assert.Equal(e.OnException, a.OnException);
      Assert.Equal(e.OnExceptionNextStep, a.OnExceptionNextStep);
      Assert.Equal(e.Next, a.Next);
      Assert.Equal(e.Transitions, a.Transitions);
    }
  }
}
