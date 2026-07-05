namespace WorkflowEngine;

public sealed class StepResult
{
  public bool Success { get; init; }

  public string? NextStep { get; init; }

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
