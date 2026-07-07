namespace WorkflowEngine;

public sealed class StepResult
{
  public bool Success { get; init; }

  public string? NextStep { get; init; }

  /// <summary>Benannter Ausgang des Steps (z.B. "StopComplete"). Wird über die im Workflow
  /// definierten Transitions auf den tatsächlichen Ziel-Step aufgelöst.</summary>
  public string? Outcome { get; init; }

  public object? Result { get; init; }

  public Exception? Exception { get; init; }

  public static StepResult Ok(string? nextStep = null, object? result = null)
  {
    return new StepResult
    {
      Success = true,
      NextStep = nextStep,
      Result = result
    };
  }

  public static StepResult Continue(string nextStep)
  {
    return Ok(nextStep);
  }

  /// <summary>Meldet einen benannten Ausgang, dessen Ziel-Step über die Workflow-Transitions aufgelöst wird.</summary>
  public static StepResult Branch(string outcome, object? result = null)
  {
    if (string.IsNullOrWhiteSpace(outcome))
    {
      throw new ArgumentException("Outcome darf nicht leer sein.", nameof(outcome));
    }

    return new StepResult
    {
      Success = true,
      Outcome = outcome,
      Result = result
    };
  }

  public static StepResult Fail(Exception exception, string? nextStep = null, object? result = null)
  {
    return new StepResult
    {
      Success = false,
      NextStep = nextStep,
      Result = result,
      Exception = exception
    };
  }
}
