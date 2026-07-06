using WorkflowEngine.Blazor;
using WorkflowEngine.Definition;
using Work.Workflows.Pages;
using Work.Workflows.Steps;

namespace Work.Workflows;

public static class UserWizardWorkflow
{
  public static WorkflowDefinition Build()
  {
    return new WorkflowBuilder("UserWizard")
      .StartWith("Start")
      .Step("Start")
        .Component<StartPage>()
      .Step("SelectUser")
        .Component<SelectUserPage>()
      .Step("CreateUser")
        .Before(context =>
        {
          context.Set("CreateUser.Attempts", 0);
        })
        .Component<CreateUserPage>()
        .Execute<CreateUserStep>()
        .After(context =>
        {
          context.Set("CreateUser.LastRunUtc", DateTime.UtcNow);
        })
      .Step("ConfigureRoles")
        .Component<ConfigureRolesPage>()
      .Step("Summary")
        .Component<SummaryPage>()
      .Step("Advanced")
        .Component<AdvancedPage>()
      .Step("Finish")
        .Execute<FinishStep>()
        .Retry(3)
        .OnException((context, exception) =>
        {
          context.Set("Finish.Error", exception.Message);
          return "Advanced";
        })
      .Build();
  }
}
