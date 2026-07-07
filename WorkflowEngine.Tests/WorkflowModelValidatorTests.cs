using WorkflowEngine.Catalog;
using WorkflowEngine.Configuration;
using WorkflowEngine.Tests.Fixtures;
using WorkflowEngine.Validation;
using Work.Workflows.DriverTour;
using Work.Workflows;
using Xunit;

namespace WorkflowEngine.Tests;

public class WorkflowModelValidatorTests
{
  private static string RepositoryRoot =>
    Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));

  private static IBlockCatalog BuildWorkCatalog() =>
    new BlockCatalogBuilder().Build(typeof(UserWizardWorkflow).Assembly);

  private static IBlockCatalog BuildFixtureCatalog() =>
    new BlockCatalogBuilder().Build(typeof(ProducesFooStep).Assembly);

  private static WorkflowConfigurationModel ReadRealModel(string configFileRelativePath)
  {
    var jsonPath = Path.Combine(RepositoryRoot, "Work", configFileRelativePath);
    return new JsonWorkflowConfigurationReader().Read(File.ReadAllText(jsonPath));
  }

  [Fact]
  public void Validate_DriverTourWorkflowJson_HasNoErrors()
  {
    var issues = new WorkflowModelValidator().Validate(
      ReadRealModel(DriverTourWorkflowConfig.ConfigFileRelativePath), BuildWorkCatalog());

    Assert.DoesNotContain(issues, i => i.Severity == ValidationSeverity.Error);

    // TourEnd ist das bewusste Workflow-Ende -> einzige erwartete Warnung ("keine ausgehende Kante").
    var warning = Assert.Single(issues);
    Assert.Equal(ValidationSeverity.Warning, warning.Severity);
    Assert.Equal("TourEnd", warning.StepName);
  }

  [Fact]
  public void Validate_UserWizardWorkflowJson_HasNoErrors()
  {
    var issues = new WorkflowModelValidator().Validate(
      ReadRealModel(UserWizardWorkflowConfig.ConfigFileRelativePath), BuildWorkCatalog());

    Assert.DoesNotContain(issues, i => i.Severity == ValidationSeverity.Error);

    // Finish ist das bewusste Workflow-Ende -> einzige erwartete Warnung.
    var warning = Assert.Single(issues);
    Assert.Equal(ValidationSeverity.Warning, warning.Severity);
    Assert.Equal("Finish", warning.StepName);
  }

  [Fact]
  public void Validate_DetectsMissingRequiredInput()
  {
    var model = new WorkflowConfigurationModel
    {
      Name = "Test",
      StartStep = "A",
      Steps = { new WorkflowStepConfigurationModel { Name = "A", Execute = nameof(RequiresFooStep) } }
    };

    var issues = new WorkflowModelValidator().Validate(model, BuildFixtureCatalog());

    Assert.Contains(issues, i =>
      i.Severity == ValidationSeverity.Error && i.StepName == "A" && i.Message.Contains("Foo"));
  }

  [Fact]
  public void Validate_DoesNotFlagRequiredInputProducedByAllIncomingPaths()
  {
    var model = new WorkflowConfigurationModel
    {
      Name = "Test",
      StartStep = "Producer",
      Steps =
      {
        new WorkflowStepConfigurationModel
        {
          Name = "Producer",
          Execute = nameof(ProducesFooStep),
          Next = "Consumer"
        },
        new WorkflowStepConfigurationModel { Name = "Consumer", Execute = nameof(RequiresFooStep) }
      }
    };

    var issues = new WorkflowModelValidator().Validate(model, BuildFixtureCatalog());

    Assert.DoesNotContain(issues, i => i.Severity == ValidationSeverity.Error);
  }

  [Fact]
  public void Validate_DetectsDeadTargetName()
  {
    var model = new WorkflowConfigurationModel
    {
      Name = "Test",
      StartStep = "A",
      Steps =
      {
        new WorkflowStepConfigurationModel { Name = "A", Execute = nameof(ProducesFooStep), Next = "DoesNotExist" }
      }
    };

    var issues = new WorkflowModelValidator().Validate(model, BuildFixtureCatalog());

    Assert.Contains(issues, i =>
      i.Severity == ValidationSeverity.Error && i.StepName == "A" && i.Message.Contains("DoesNotExist"));
  }

  [Fact]
  public void Validate_DetectsUnreachableStep()
  {
    var model = new WorkflowConfigurationModel
    {
      Name = "Test",
      StartStep = "A",
      Steps =
      {
        new WorkflowStepConfigurationModel { Name = "A", Execute = nameof(ProducesFooStep) },
        new WorkflowStepConfigurationModel { Name = "B", Execute = nameof(ProducesFooStep) }
      }
    };

    var issues = new WorkflowModelValidator().Validate(model, BuildFixtureCatalog());

    Assert.Contains(issues, i =>
      i.Severity == ValidationSeverity.Warning && i.StepName == "B" && i.Message.Contains("nicht erreichbar"));
  }

  [Fact]
  public void Validate_DetectsUnknownStartStep()
  {
    var model = new WorkflowConfigurationModel
    {
      Name = "Test",
      StartStep = "DoesNotExist",
      Steps = { new WorkflowStepConfigurationModel { Name = "A", Execute = nameof(ProducesFooStep) } }
    };

    var issues = new WorkflowModelValidator().Validate(model, BuildFixtureCatalog());

    Assert.Contains(issues, i => i.Severity == ValidationSeverity.Error && i.Message.Contains("StartStep"));
  }

  [Fact]
  public void Validate_DetectsUnknownAlias()
  {
    var model = new WorkflowConfigurationModel
    {
      Name = "Test",
      StartStep = "A",
      Steps = { new WorkflowStepConfigurationModel { Name = "A", Execute = "DoesNotExistStep" } }
    };

    var issues = new WorkflowModelValidator().Validate(model, BuildFixtureCatalog());

    Assert.Contains(issues, i =>
      i.Severity == ValidationSeverity.Error && i.StepName == "A" && i.Message.Contains("Katalog"));
  }
}
