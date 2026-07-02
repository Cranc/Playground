namespace Work.Components.TicketPlanner.Models;

/// <summary>
/// Represents a ticket in the planner with its properties such as Id, Key, Title, Type, Status, Priority, and EstimatedHours.
/// </summary>
public class PlannerTicket
{
  /// <summary>
  /// Identifier for the ticket, used to uniquely identify it within the planner.
  /// </summary>
  public string Id { get; set; } = "";
  /// <summary>
  /// Key for the ticket, which may be used for referencing or categorization purposes.
  /// </summary>
  public string Key { get; set; } = "";
  /// <summary>
  /// Title of the ticket, providing a brief description or summary of the task or issue it represents.
  /// </summary>
  public string Title { get; set; } = "";
  /// <summary>
  /// Type of the ticket, indicating the nature or category of the task or issue it represents (e.g., Bug, Feature, Task).
  /// </summary>
  public string Type { get; set; } = "";
  /// <summary>
  /// Status of the ticket (e.g., Ready, Backlog, In Progress).
  /// </summary>
  public string Status { get; set; } = "";
  /// <summary>
  /// Priority of the ticket, indicating its level of importance or urgency (e.g., Low, Medium, High).
  /// </summary>
  public string Priority { get; set; } = "";
  /// <summary>
  /// Estimated hours required to complete the ticket, providing an estimate of the time needed for planning and resource allocation.
  /// </summary>
  public decimal EstimatedHours { get; set; }
}

/// <summary>
/// Represents a member in the planner with properties such as Id, DisplayName, Role, MonthlyCapacityHours, and Color.
/// </summary>
public class PlannerMember
{
  /// <summary>
  /// Identifier for the member, used to uniquely identify them within the planner.
  /// </summary>
  public string Id { get; set; } = "";
  /// <summary>
  /// Display name of the member, providing a human-readable name for identification and display purposes.
  /// </summary>
  public string DisplayName { get; set; } = "";
  /// <summary>
  /// Role of the member, indicating their position or responsibilities within the team or organization (e.g., Developer, Designer, Manager).
  /// </summary>
  public string Role { get; set; } = "";
  /// <summary>
  /// Monthly capacity hours for the member, indicating the total number of hours they are available to work on tasks or tickets within a month.
  /// </summary>
  public decimal MonthlyCapacityHours { get; set; } = 40;
  /// <summary>
  /// Color associated with the member, which can be used for visual representation in the planner (e.g., for color-coding tasks or assignments).
  /// </summary>
  public string Color { get; set; } = "#00D3A7";
}

/// <summary>
/// Represents an assignment of a ticket to a member in the planner, including the ticket ID, member ID, and planned hours for the assignment.
/// </summary>
public class PlannerAssignment
{
  /// <summary>
  /// Ticket ID associated with the assignment, indicating which ticket is being assigned to the member.
  /// </summary>
  public string TicketId { get; set; } = "";
  /// <summary>
  /// Member ID associated with the assignment, indicating which member is being assigned to the ticket.
  /// </summary>
  public string MemberId { get; set; } = "";
  /// <summary>
  /// Planned hours for the assignment, indicating the estimated number of hours the member is expected to spend on the assigned ticket.
  /// </summary>
  public decimal PlannedHours { get; set; }
}

/// <summary>
/// Represents a plan in the planner, containing properties such as Id, Name, Members, and Assignments. The Members property holds a list of PlannerMember objects, while the Assignments property holds a list of PlannerAssignment objects.
/// </summary>
public class PlannerPlan
{
  /// <summary>
  /// Identifier for the plan, used to uniquely identify it within the planner.
  /// </summary>
  public string Id { get; set; } = "";
  /// <summary>
  /// Name of the plan, providing a human-readable title or description for identification and display purposes.
  /// </summary>
  public string Name { get; set; } = "";
  /// <summary>
  /// List of members associated with the plan, represented by PlannerMember objects. This property allows for the management and tracking of team members involved in the plan.
  /// </summary>
  public List<PlannerMember> Members { get; set; } = new();
  /// <summary>
  /// List of assignments associated with the plan, represented by PlannerAssignment objects. This property allows for the management and tracking of ticket assignments to team members within the plan.
  /// </summary>
  public List<PlannerAssignment> Assignments { get; set; } = new();
}