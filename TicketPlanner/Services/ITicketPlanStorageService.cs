using System.Threading.Tasks;
using TicketPlanner.Models;

namespace TicketPlanner.Services;

/// <summary>
/// Defines persistence operations for saving, loading, and exporting planner state.
/// </summary>
public interface ITicketPlanStorageService
{
    /// <summary>
    /// Persists the current planner state.
    /// </summary>
    /// <param name="plan">The planner state to save.</param>
    Task SavePlanAsync(PlannerPlan plan);

    /// <summary>
    /// Loads a previously saved planner state.
    /// </summary>
    /// <returns>The stored planner state, or <see langword="null"/> when none exists.</returns>
    Task<PlannerPlan?> LoadPlanAsync();

    /// <summary>
    /// Exports the provided planner state as a downloadable JSON file.
    /// </summary>
    /// <param name="plan">The planner state to export.</param>
    /// <param name="filename">The name of the exported file.</param>
    Task ExportPlanAsync(PlannerPlan plan, string filename = "ticketplanner.json");
}
