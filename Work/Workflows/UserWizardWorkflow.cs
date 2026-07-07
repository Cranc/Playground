using WorkflowEngine.Blazor;
using WorkflowEngine.Configuration;
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
        .On("SelectUser", "SelectUser")
        .On("CreateUser", "CreateUser")
      .Step("SelectUser")
        .Component<SelectUserPage>()
        .Next("ConfigureRoles")
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
        .Next("ConfigureRoles")
      .Step("ConfigureRoles")
        .Component<ConfigureRolesPage>()
        .On("Summary", "Summary")
        .On("Advanced", "Advanced")
      .Step("Summary")
        .Component<SummaryPage>()
        .On("Finish", "Finish")
        .On("Advanced", "Advanced")
      .Step("Advanced")
        .Component<AdvancedPage>()
        .On("Finish", "Finish")
        .On("Summary", "Summary")
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

  /// <summary>
  /// Baut denselben Workflow wie <see cref="Build"/>, jedoch vollständig aus der externen
  /// JSON-Konfiguration (<c>UserWizardWorkflow.json</c>) inklusive Before-/After-Aktionen,
  /// Retry und Exception-Handler.
  /// </summary>
  public static WorkflowDefinition BuildFromConfig()
  {
    var registry = UserWizardWorkflowConfig.CreateRegistry();
    var loader = new WorkflowConfigurationLoader();
    return loader.LoadFromFile(UserWizardWorkflowConfig.ConfigFilePath, registry);
  }
}
