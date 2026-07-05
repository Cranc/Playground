using System.Threading.Tasks;
using Work.Components.TicketPlanner.Models;

namespace Work.Components.TicketPlanner.Services;

public interface ITicketPlanStorageService
{
    Task SavePlanAsync(PlannerPlan plan);
    Task<PlannerPlan?> LoadPlanAsync();
    Task ExportPlanAsync(PlannerPlan plan, string filename = "ticketplanner.json");
}
