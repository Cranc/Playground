namespace WorkflowEngine;

public interface IWorkflowStep
{
  Task<StepResult> ExecuteAsync(WorkflowContext context);
}
