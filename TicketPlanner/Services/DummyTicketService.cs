namespace TicketPlanner.Services;

using TicketPlanner.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

public class DummyTicketService : ITicketService
{
  public Task<IEnumerable<PlannerTicket>> GetTicketsAsync()
  {
    var list = new List<PlannerTicket>
    {
      new PlannerTicket { Id = "1", Key = "#2600001", Title = "Dashboard analytics redesign", Type = "Feature", Priority = "High", Status = "Ready", EstimatedHours = 16 },
      new PlannerTicket { Id = "2", Key = "#2600002", Title = "API rate limiting implementation", Type = "Task", Priority = "High", Status = "Backlog", EstimatedHours = 12 },
      new PlannerTicket { Id = "3", Key = "#2600003", Title = "Onboarding flow user research", Type = "Spike", Priority = "Medium", Status = "Ready", EstimatedHours = 0 },
      new PlannerTicket { Id = "4", Key = "#2600004", Title = "CSV export UTF-8 encoding bug", Type = "Bug", Priority = "Medium", Status = "Backlog", EstimatedHours = 4 },
      new PlannerTicket { Id = "5", Key = "#2600005", Title = "Dark mode token system", Type = "Feature", Priority = "Medium", Status = "Ready", EstimatedHours = 20 },
      new PlannerTicket { Id = "6", Key = "#2600006", Title = "Payment webhook retry logic", Type = "Task", Priority = "High", Status = "Ready", EstimatedHours = 10 },
      new PlannerTicket { Id = "7", Key = "#2600007", Title = "Notification preferences page", Type = "Feature", Priority = "Low", Status = "Backlog", EstimatedHours = 14 },
      new PlannerTicket { Id = "8", Key = "#2600008", Title = "Performance audit — home page", Type = "Spike", Priority = "Medium", Status = "Backlog", EstimatedHours = 6 },
      new PlannerTicket { Id = "9", Key = "#2600009", Title = "GDPR data export endpoint", Type = "Task", Priority = "Critical", Status = "Ready", EstimatedHours = 12 }
    };

    return Task.FromResult<IEnumerable<PlannerTicket>>(list);
  }
}
