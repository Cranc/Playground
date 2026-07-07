using WorkflowEngine.Configuration;
using Work.Workflows;
using Work.Workflows.DriverTour;
using Xunit;

namespace WorkflowEngine.Tests;

/// <summary>
/// Stellt sicher, dass die auf den Baustein-Katalog umgestellten *WorkflowConfig-Klassen
/// (Phase 1.4) weiterhin dieselben Aliase liefern wie zuvor die manuelle Registry, sodass
/// die bestehenden JSON-Konfigurationen unverändert laden.
/// </summary>
public class WorkflowConfigRegistryRegressionTests
{
  private static string RepositoryRoot =>
    Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));

  [Fact]
  public void DriverTourWorkflow_LoadsFromJson_WithCatalogBasedRegistry()
  {
    var registry = DriverTourWorkflowConfig.CreateRegistry();
    var jsonPath = Path.Combine(RepositoryRoot, "Work", DriverTourWorkflowConfig.ConfigFileRelativePath);

    var definition = new WorkflowConfigurationLoader().LoadFromFile(jsonPath, registry);

    Assert.Equal("DriverTour", definition.Name);
    Assert.Equal("TourStart", definition.StartStep);
    Assert.Equal(11, definition.Steps.Count);
  }

  [Fact]
  public void UserWizardWorkflow_LoadsFromJson_WithCatalogBasedRegistryAndHooks()
  {
    var registry = UserWizardWorkflowConfig.CreateRegistry();
    var jsonPath = Path.Combine(RepositoryRoot, "Work", UserWizardWorkflowConfig.ConfigFileRelativePath);

    var definition = new WorkflowConfigurationLoader().LoadFromFile(jsonPath, registry);

    Assert.Equal("UserWizard", definition.Name);
    Assert.Equal("Start", definition.StartStep);
    Assert.Equal(7, definition.Steps.Count);
  }
}
