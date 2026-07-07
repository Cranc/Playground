using WorkflowEngine.Catalog;
using Xunit;

namespace WorkflowEngine.Tests;

public class BlockCatalogBuilderTests
{
  private static IBlockCatalog BuildWorkCatalog()
  {
    return new BlockCatalogBuilder().Build(typeof(Work.Workflows.UserWizardWorkflow).Assembly);
  }

  [Fact]
  public void Build_ScansAllDriverTourAndUserWizardBlocks()
  {
    var catalog = BuildWorkCatalog();

    // 5 DriverTour-Steps + 8 DriverTour-Pages (inkl. ungenutzter TourStartPage) +
    // 2 UserWizard-Steps + 6 UserWizard-Pages = 21 Bausteine.
    Assert.Equal(21, catalog.Blocks.Count);
  }

  [Theory]
  [InlineData("InitializeTourStep", BlockKind.Action)]
  [InlineData("CompleteShipmentStep", BlockKind.Action)]
  [InlineData("StopArrivalPage", BlockKind.Page)]
  [InlineData("CreateUserStep", BlockKind.Action)]
  [InlineData("StartPage", BlockKind.Page)]
  public void Build_ResolvesKnownAliasesWithExpectedKind(string alias, BlockKind expectedKind)
  {
    var catalog = BuildWorkCatalog();

    Assert.True(catalog.TryGet(alias, out var descriptor));
    Assert.Equal(expectedKind, descriptor.Kind);
  }

  [Fact]
  public void Build_ReadsConsumesAndProducesPortsForCompleteShipmentStep()
  {
    var catalog = BuildWorkCatalog();

    Assert.True(catalog.TryGet("CompleteShipmentStep", out var descriptor));
    Assert.Contains(descriptor.Inputs, p => p.Key == "Tour.CurrentShipmentId" && p.Required);
    Assert.Contains(descriptor.Inputs, p => p.Key == "Shipment.PendingSignature" && !p.Required);
    Assert.Contains(descriptor.Outcomes, o => o.Name == "StopComplete");
    Assert.Contains(descriptor.Outcomes, o => o.Name == "ShipmentSelection");
  }

  [Fact]
  public void Build_AssignsImplicitDefaultOutcomeForPagesWithoutExplicitOutcomes()
  {
    var catalog = BuildWorkCatalog();

    Assert.True(catalog.TryGet("CreateUserPage", out var descriptor));
    Assert.Equal(BlockKind.Page, descriptor.Kind);
    Assert.Single(descriptor.Outcomes);
    Assert.Equal("Default", descriptor.Outcomes[0].Name);
  }

  [Fact]
  public void FromCatalog_ProducesRegistryThatResolvesAllConfiguredAliases()
  {
    var catalog = BuildWorkCatalog();
    var registry = Configuration.WorkflowTypeRegistry.FromCatalog(catalog);

    Assert.True(registry.TryResolveStep("InitializeTourStep", out _));
    Assert.True(registry.TryResolveComponent("StopArrivalPage", out _));
    Assert.True(registry.TryResolveStep("CreateUserStep", out _));
    Assert.True(registry.TryResolveComponent("StartPage", out _));
  }
}
