namespace TicketPlanner.Services;

using TicketPlanner.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

public interface ITicketService
{
  Task<IEnumerable<PlannerTicket>> GetTicketsAsync();
}
