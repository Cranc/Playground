using WorkflowEngine;
using WorkflowEngine.Catalog;

namespace Work.Workflows.DriverTour.Steps;

/// <summary>
/// Erzeugt beim (Neu-)Start des Workflows die Tour-Dummy-Daten, sofern noch keine im
/// Kontext vorhanden sind, und springt direkt zum ersten Stopp. Läuft automatisch ohne
/// eigene UI, da die "Tour starten"-Ansicht bereits außerhalb des Workflows (Popup-Trigger) liegt.
/// </summary>
[WorkflowBlock(DisplayName = "Tour initialisieren", Category = "Fahrer-Tour", Description = "Erzeugt die Tourdaten, falls noch keine vorhanden sind.")]
[ProducesContext("Tour", typeof(Tour))]
[ProducesContext("Tour.CurrentStopIndex", typeof(int))]
public sealed class InitializeTourStep : IWorkflowStep
{
  public Task<StepResult> ExecuteAsync(WorkflowContext context)
  {
    if (!context.TryGet<Tour>("Tour", out var tour) || tour is null)
    {
      tour = DriverTourSeedData.CreateDummyTour();
      context.Set("Tour", tour);
      context.Set("Tour.CurrentStopIndex", 0);
    }

    return Task.FromResult(StepResult.Ok());
  }
}
