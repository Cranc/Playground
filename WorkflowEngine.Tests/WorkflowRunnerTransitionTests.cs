using Microsoft.Extensions.DependencyInjection;
using WorkflowEngine.Definition;
using WorkflowEngine.Execution;
using Work.Workflows;
using Work.Workflows.DriverTour;
using Xunit;

namespace WorkflowEngine.Tests;

/// <summary>
/// Phase 2: Verifiziert, dass DriverTour und UserWizard ausschließlich über Outcome/Transition-
/// Daten laufen (kein hartkodierter Zielname mehr im Step-/Page-Code) und dass der Runner bei
/// einem unbekannten Outcome aussagekräftig fehlschlägt. Läuft für beide Erzeugungswege
/// (fluent Build() und JSON BuildFromConfig()), damit beide Pfade dieselbe Definition ergeben.
/// </summary>
public class WorkflowRunnerTransitionTests
{
  private static WorkflowContext NewContext() => new(new ServiceCollection().BuildServiceProvider());

  [Fact]
  public Task DriverTourWorkflow_FullRun_CodeBuilder() => RunFullDriverTourAsync(DriverTourWorkflow.Build());

  [Fact]
  public Task DriverTourWorkflow_FullRun_JsonConfig() => RunFullDriverTourAsync(DriverTourWorkflow.BuildFromConfig());

  [Fact]
  public Task UserWizardWorkflow_FullRun_CodeBuilder() => RunFullUserWizardAsync(UserWizardWorkflow.Build());

  [Fact]
  public Task UserWizardWorkflow_FullRun_JsonConfig() => RunFullUserWizardAsync(UserWizardWorkflow.BuildFromConfig());

  /// <summary>
  /// Fährt alle drei Stopps der Dummy-Tour ab: Stopp 1 per Einzelbearbeitung (inkl. Signatur-
  /// Pflicht bei einer Sendung), Stopp 2+3 per Sammelabschluss. Deckt damit unbedingte
  /// Next-Übergänge, daten- und UI-getriebene Outcomes sowie den gemeinsamen Execute+Component-
  /// Step (LoadCarrierBooking) ab.
  /// </summary>
  private static async Task RunFullDriverTourAsync(WorkflowDefinition definition)
  {
    var context = NewContext();
    var runner = new WorkflowRunner(definition, context);

    await runner.StartAsync();
    Assert.Equal("StopArrival", runner.CurrentStepName);

    // Stopp 1 (2 Sendungen) per Einzelbearbeitung.
    await runner.AdvanceByOutcomeAsync("ShipmentSelection");
    Assert.Equal("ShipmentSelection", runner.CurrentStepName);

    context.Set("Tour.CurrentShipmentId", "SDG-1001");
    await runner.AdvanceByOutcomeAsync("ShipmentStatus");
    Assert.Equal("ShipmentStatus", runner.CurrentStepName);

    context.Set("Shipment.PendingStatus", ShipmentStatus.ErfolgreichZugestellt); // erfordert Signatur
    await runner.AdvanceByOutcomeAsync("Signature");
    Assert.Equal("Signature", runner.CurrentStepName);

    context.Set("Shipment.PendingSignature", "Max Mustermann");
    await runner.AdvanceAsync(); // Signature.Next -> CompleteShipment (Branch) -> zurück zu ShipmentSelection
    Assert.Equal("ShipmentSelection", runner.CurrentStepName);

    context.Set("Tour.CurrentShipmentId", "SDG-1002");
    await runner.AdvanceByOutcomeAsync("ShipmentStatus");
    context.Set("Shipment.PendingStatus", ShipmentStatus.PackstueckBeschaedigt); // keine Signatur nötig
    await runner.AdvanceByOutcomeAsync("CompleteShipment"); // CompleteShipment (Branch: StopComplete) -> StopComplete -> LoadCarrierBooking
    Assert.Equal("LoadCarrierBooking", runner.CurrentStepName);

    await runner.AdvanceAsync(); // AdvanceStopStep (Branch: StopArrival, da noch 2 Stopps offen)
    Assert.Equal("StopArrival", runner.CurrentStepName);
    Assert.Equal(1, context.Get<int>("Tour.CurrentStopIndex"));

    // Stopp 2 (1 Sendung) per Sammelabschluss.
    await runner.AdvanceByOutcomeAsync("BulkShipmentStatus");
    context.Set("Stop.BulkStatus", ShipmentStatus.KundeNichtAngetroffen);
    await runner.AdvanceAsync(); // BulkShipmentStatus.Next -> CompleteBulkShipments -> StopComplete -> LoadCarrierBooking
    Assert.Equal("LoadCarrierBooking", runner.CurrentStepName);

    await runner.AdvanceAsync(); // AdvanceStopStep (Branch: StopArrival, noch 1 Stopp offen)
    Assert.Equal("StopArrival", runner.CurrentStepName);
    Assert.Equal(2, context.Get<int>("Tour.CurrentStopIndex"));

    // Stopp 3 (letzter Stopp) per Sammelabschluss.
    await runner.AdvanceByOutcomeAsync("BulkShipmentStatus");
    context.Set("Stop.BulkStatus", ShipmentStatus.Sonstiges);
    await runner.AdvanceAsync();
    Assert.Equal("LoadCarrierBooking", runner.CurrentStepName);

    await runner.AdvanceAsync(); // AdvanceStopStep (Branch: TourEnd, keine Stopps mehr offen)
    Assert.Equal("TourEnd", runner.CurrentStepName);
    Assert.False(runner.IsCompleted);

    context.Set("Tour.CompletedAtUtc", DateTime.UtcNow);
    await runner.AdvanceAsync(); // TourEnd hat kein Next -> Workflow abgeschlossen
    Assert.True(runner.IsCompleted);

    var tour = context.Get<Tour>("Tour");
    Assert.All(tour.Stops, stop => Assert.True(stop.IsCompleted));
    Assert.All(tour.Stops.SelectMany(s => s.Shipments), shipment => Assert.True(shipment.IsCompleted));
  }

  private static async Task RunFullUserWizardAsync(WorkflowDefinition definition)
  {
    var context = NewContext();
    var runner = new WorkflowRunner(definition, context);

    await runner.StartAsync();
    Assert.Equal("Start", runner.CurrentStepName);

    await runner.AdvanceByOutcomeAsync("CreateUser");
    Assert.Equal("CreateUser", runner.CurrentStepName);

    context.Set("User.Name", "Test User");
    context.Set("User.Email", "test@example.com");
    await runner.AdvanceAsync(); // Before/Execute<CreateUserStep>/After, danach Next -> ConfigureRoles
    Assert.Equal("ConfigureRoles", runner.CurrentStepName);
    Assert.Equal(1, context.Get<int>("CreateUser.Attempts"));

    context.Set("User.Roles", new List<string> { "Admin" });
    await runner.AdvanceByOutcomeAsync("Summary");
    Assert.Equal("Summary", runner.CurrentStepName);

    await runner.AdvanceByOutcomeAsync("Finish"); // Finish hat kein Component -> läuft automatisch durch
    Assert.True(runner.IsCompleted);
    Assert.True(context.Get<bool>("Wizard.Finished"));
  }

  [Fact]
  public async Task StartAsync_ThrowsWhenStepResultOutcomeHasNoTransition()
  {
    var definition = new WorkflowBuilder("Test")
      .StartWith("A")
      .Step("A")
        .Execute(_ => Task.FromResult(StepResult.Branch("Unmapped")))
      .Step("B")
        .Execute(_ => Task.FromResult(StepResult.Ok()))
      .Build();

    var runner = new WorkflowRunner(definition, NewContext());

    var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => runner.StartAsync());
    Assert.Contains("Unmapped", ex.Message);
  }

  [Fact]
  public async Task AdvanceByOutcomeAsync_ThrowsWhenNoTransitionForOutcome()
  {
    var definition = new WorkflowBuilder("Test")
      .StartWith("A")
      .Step("A")
        .WithComponent(typeof(object)) // beliebiger Marker, der die automatische Traversierung stoppt
      .Step("B")
        .Execute(_ => Task.FromResult(StepResult.Ok()))
      .Build();

    var runner = new WorkflowRunner(definition, NewContext());
    await runner.StartAsync();

    await Assert.ThrowsAsync<InvalidOperationException>(() => runner.AdvanceByOutcomeAsync("DoesNotExist"));
  }
}
