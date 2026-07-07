using WorkflowEngine;
using WorkflowEngine.Catalog;

namespace Work.Workflows.Steps;

[WorkflowBlock(DisplayName = "Benutzer anlegen", Category = "User-Wizard", Description = "Legt den Benutzer anhand von Name und E-Mail an.")]
[ConsumesContext("User.Name", typeof(string))]
[ConsumesContext("User.Email", typeof(string), Required = false)]
[ConsumesContext("CreateUser.Attempts", typeof(int), Required = false)]
[ProducesContext("CreateUser.Attempts", typeof(int))]
[ProducesContext("User.Created", typeof(object))]
[Outcome("ConfigureRoles", DisplayName = "Rollen konfigurieren")]
public sealed class CreateUserStep : IWorkflowStep
{
  public Task<StepResult> ExecuteAsync(WorkflowContext context)
  {
    context.TryGet<int>("CreateUser.Attempts", out var attempts);
    context.Set("CreateUser.Attempts", attempts + 1);

    if (!context.TryGet<string>("User.Name", out var userName) || string.IsNullOrWhiteSpace(userName))
    {
      throw new InvalidOperationException("Es wurde kein Benutzername angegeben.");
    }

    context.TryGet<string>("User.Email", out var email);
    var createdUser = new
    {
      Name = userName,
      Email = email,
      CreatedAtUtc = DateTime.UtcNow
    };

    context.Set("User.Created", createdUser);
    return Task.FromResult(StepResult.Ok(nextStep: "ConfigureRoles", result: createdUser));
  }
}
