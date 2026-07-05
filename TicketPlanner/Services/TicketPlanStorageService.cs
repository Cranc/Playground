using Microsoft.JSInterop;
using System.Text.Json;
using TicketPlanner.Constants;
using TicketPlanner.Models;

namespace TicketPlanner.Services;

public class TicketPlanStorageService : ITicketPlanStorageService
{
  const string _storageKey = "ticketPlannerState";
  readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true, WriteIndented = true };
  readonly ILocalStorageService _localStorage;
  private readonly IJSRuntime _jsRuntime;

  public TicketPlanStorageService(ILocalStorageService localStorage, IJSRuntime jsRuntime)
  {
    _localStorage = localStorage;
    _jsRuntime = jsRuntime;
  }

  public async Task SavePlanAsync(PlannerPlan plan)
  {
    try
    {
      var json = JsonSerializer.Serialize(plan, _jsonOptions);
      await _localStorage.SetItemAsync(_storageKey, json);
    }
    catch (Exception ex)
    {
      Console.WriteLine($"TicketPlanStorageService.SavePlanAsync error: {ex}");
    }
  }

  public async Task<PlannerPlan?> LoadPlanAsync()
  {
    try
    {
      var json = await _localStorage.GetItemAsync<string>(_storageKey);
      if (string.IsNullOrEmpty(json)) return null;
      return JsonSerializer.Deserialize<PlannerPlan>(json, _jsonOptions);
    }
    catch (Exception ex)
    {
      Console.WriteLine($"TicketPlanStorageService.LoadPlanAsync error: {ex}");
      return null;
    }
  }

  public async Task ExportPlanAsync(PlannerPlan plan, string filename = "ticketplanner.json")
  {
    try
    {
      var json = JsonSerializer.Serialize(plan, _jsonOptions);
      await _jsRuntime.InvokeVoidAsync(
        JavaScriptConstants.Functions.FileOperations.ExportFile,
        filename,
        json
      );
    }
    catch (Exception ex)
    {
      Console.WriteLine($"TicketPlanStorageService.ExportPlanAsync error: {ex}");
    }
  }
}
