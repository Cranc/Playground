using System.Threading.Tasks;
using TicketPlanner.Models;

namespace TicketPlanner.Services;

public interface ITicketPlanStorageService
{
    Task SavePlanAsync(PlannerPlan plan);
    Task<PlannerPlan?> LoadPlanAsync();
    Task ExportPlanAsync(PlannerPlan plan, string filename = "ticketplanner.json");
}
