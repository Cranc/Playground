namespace Work.Components.TicketPlanner.Services;

using Work.Components.TicketPlanner.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

public interface ITicketService
{
  Task<IEnumerable<PlannerTicket>> GetTicketsAsync();
}
