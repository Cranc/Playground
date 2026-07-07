namespace WorkflowEngine.Definition;

public sealed class WorkflowStepBuilder
{
  private readonly WorkflowBuilder.WorkflowStepConfiguration _configuration;

  internal WorkflowStepBuilder(WorkflowBuilder workflowBuilder, WorkflowBuilder.WorkflowStepConfiguration configuration)
  {
    WorkflowBuilder = workflowBuilder;
    _configuration = configuration;
  }

  internal WorkflowBuilder WorkflowBuilder { get; }

  public WorkflowStepBuilder Before(Func<WorkflowContext, Task> before)
  {
    _configuration.Before = before ?? throw new ArgumentNullException(nameof(before));
    return this;
  }

  public WorkflowStepBuilder Before(Action<WorkflowContext> before)
  {
    if (before is null)
    {
      throw new ArgumentNullException(nameof(before));
    }

    return Before(context =>
    {
      before(context);
      return Task.CompletedTask;
    });
  }

  public WorkflowStepBuilder Execute<TStep>() where TStep : class, IWorkflowStep
  {
    _configuration.StepType = typeof(TStep);
    _configuration.Execute = null;
    return this;
  }

  public WorkflowStepBuilder Execute(Func<WorkflowContext, Task<StepResult>> execute)
  {
    _configuration.Execute = execute ?? throw new ArgumentNullException(nameof(execute));
    _configuration.StepType = null;
    return this;
  }

  public WorkflowStepBuilder Execute(Type stepType)
  {
    if (stepType is null)
    {
      throw new ArgumentNullException(nameof(stepType));
    }

    if (!typeof(IWorkflowStep).IsAssignableFrom(stepType))
    {
      throw new ArgumentException($"Step-Typ '{stepType.FullName}' implementiert IWorkflowStep nicht.", nameof(stepType));
    }

    _configuration.StepType = stepType;
    _configuration.Execute = null;
    return this;
  }

  public WorkflowStepBuilder After(Func<WorkflowContext, Task> after)
  {
    _configuration.After = after ?? throw new ArgumentNullException(nameof(after));
    return this;
  }

  public WorkflowStepBuilder After(Action<WorkflowContext> after)
  {
    if (after is null)
    {
      throw new ArgumentNullException(nameof(after));
    }

    return After(context =>
    {
      after(context);
      return Task.CompletedTask;
    });
  }

  public WorkflowStepBuilder Retry(int count)
  {
    if (count < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(count), "Retry count darf nicht negativ sein.");
    }

    _configuration.RetryCount = count;
    return this;
  }

  public WorkflowStepBuilder OnException(Func<WorkflowContext, Exception, Task<string?>> onException)
  {
    _configuration.OnException = onException ?? throw new ArgumentNullException(nameof(onException));
    return this;
  }

  public WorkflowStepBuilder OnException(Func<WorkflowContext, Exception, string?> onException)
  {
    if (onException is null)
    {
      throw new ArgumentNullException(nameof(onException));
    }

    return OnException((context, exception) => Task.FromResult(onException(context, exception)));
  }

  /// <summary>Ordnet einen benannten Outcome (<see cref="StepResult.Branch"/>) einem Ziel-Step zu.</summary>
  public WorkflowStepBuilder On(string outcome, string targetStep)
  {
    if (string.IsNullOrWhiteSpace(outcome))
    {
      throw new ArgumentException("Outcome darf nicht leer sein.", nameof(outcome));
    }

    if (string.IsNullOrWhiteSpace(targetStep))
    {
      throw new ArgumentException("Ziel-Step darf nicht leer sein.", nameof(targetStep));
    }

    _configuration.Transitions[outcome] = targetStep;
    return this;
  }

  /// <summary>Legt den unbedingten Ziel-Step fest (greift, wenn kein Override/Outcome/NextStep vorliegt).</summary>
  public WorkflowStepBuilder Next(string targetStep)
  {
    if (string.IsNullOrWhiteSpace(targetStep))
    {
      throw new ArgumentException("Ziel-Step darf nicht leer sein.", nameof(targetStep));
    }

    _configuration.Next = targetStep;
    return this;
  }

  public WorkflowStepBuilder WithComponent(Type componentType)
  {
    if (componentType is null)
    {
      throw new ArgumentNullException(nameof(componentType));
    }

    _configuration.ComponentType = componentType;
    return this;
  }

  public WorkflowStepBuilder Component(Type componentType)
  {
    return WithComponent(componentType);
  }

  public WorkflowStepBuilder Step(string name)
  {
    return WorkflowBuilder.Step(name);
  }

  public WorkflowDefinition Build()
  {
    return WorkflowBuilder.Build();
  }
}
