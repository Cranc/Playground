using WorkflowEngine;

namespace Work.Workflows.Steps;

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
