namespace TicketPlanner.Services;

using TicketPlanner.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

/// <summary>
/// Provides ticket data for the planner UI.
/// </summary>
public interface ITicketService
{
  /// <summary>
  /// Returns the collection of tickets that can be assigned in the planner.
  /// </summary>
  /// <returns>A sequence of planner tickets.</returns>
  Task<IEnumerable<PlannerTicket>> GetTicketsAsync();
}
