namespace Work.Components.TicketPlanner.Services;

using Work.Components.TicketPlanner.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

public class DummyTicketService : ITicketService
{
  public Task<IEnumerable<PlannerTicket>> GetTicketsAsync()
  {
    var list = new List<PlannerTicket>
    {
      new PlannerTicket { Id = "1", Key = "PROJ-102", Title = "Dashboard analytics redesign", Type = "Feature", Priority = "High", Status = "Ready", EstimatedHours = 16 },
      new PlannerTicket { Id = "2", Key = "PROJ-103", Title = "API rate limiting implementation", Type = "Task", Priority = "High", Status = "Backlog", EstimatedHours = 12 },
      new PlannerTicket { Id = "3", Key = "PROJ-104", Title = "Onboarding flow user research", Type = "Spike", Priority = "Medium", Status = "Ready", EstimatedHours = 8 },
      new PlannerTicket { Id = "4", Key = "PROJ-105", Title = "CSV export UTF-8 encoding bug", Type = "Bug", Priority = "Medium", Status = "Backlog", EstimatedHours = 4 },
      new PlannerTicket { Id = "5", Key = "PROJ-106", Title = "Dark mode token system", Type = "Feature", Priority = "Medium", Status = "Ready", EstimatedHours = 20 },
      new PlannerTicket { Id = "6", Key = "PROJ-107", Title = "Payment webhook retry logic", Type = "Task", Priority = "High", Status = "Ready", EstimatedHours = 10 },
      new PlannerTicket { Id = "7", Key = "PROJ-108", Title = "Notification preferences page", Type = "Feature", Priority = "Low", Status = "Backlog", EstimatedHours = 14 },
      new PlannerTicket { Id = "8", Key = "PROJ-109", Title = "Performance audit — home page", Type = "Spike", Priority = "Medium", Status = "Backlog", EstimatedHours = 6 },
      new PlannerTicket { Id = "9", Key = "PROJ-110", Title = "GDPR data export endpoint", Type = "Task", Priority = "Critical", Status = "Ready", EstimatedHours = 12 }
    };

    return Task.FromResult<IEnumerable<PlannerTicket>>(list);
  }
}
