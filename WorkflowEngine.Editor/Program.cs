using System.Reflection;
using WorkflowEngine.Configuration;
using WorkflowEngine.DependencyInjection;
using WorkflowEngine.Editor.Components;
using WorkflowEngine.Editor.Services;
using WorkflowEngine.Validation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDevExpressBlazor();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var blockAssembly = Assembly.LoadFrom(ResolveBlockAssemblyPath(builder.Configuration));
builder.Services.AddWorkflowEngine(blockAssembly);

builder.Services.AddSingleton<IWorkflowFileService>(
  new WorkflowFileService(ResolveWorkflowJsonDirectory(builder.Configuration)));
builder.Services.AddSingleton<WorkflowModelValidator>();

// Für das Export-Gate: eine Registry, die alle Katalog-Bausteine (Execute/Component) auflösen
// kann. Hook-Aliase (Before/After/OnException) sind hier bewusst NICHT registriert, da sie
// nicht Teil des Katalogs sind (siehe Phase 1) — Modelle, die Hooks nutzen, überspringen diesen
// zusätzlichen Gate-Schritt (siehe ExportService).
builder.Services.AddSingleton(sp => WorkflowTypeRegistry.FromCatalog(sp.GetRequiredService<WorkflowEngine.Catalog.IBlockCatalog>()));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
  app.UseExceptionHandler("/Error", createScopeForErrors: true);
  app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

static string ResolveRepositoryRoot() =>
  Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));

/// <summary>
/// Ermittelt den Pfad zur Assembly mit den konkreten Bausteinen (Work.dll). Wird bewusst nicht
/// als ProjectReference eingebunden (siehe WorkflowEngine.Editor.csproj), sondern zur Laufzeit
/// per Reflection geladen. Der Pfad ist über "BlockAssemblyPath" konfigurierbar; ohne
/// Konfiguration wird die Standard-Build-Ausgabe des Work-Projekts im selben Repository erwartet.
/// </summary>
static string ResolveBlockAssemblyPath(IConfiguration configuration)
{
  var configuredPath = configuration["BlockAssemblyPath"];
  if (!string.IsNullOrWhiteSpace(configuredPath))
  {
    return Path.GetFullPath(configuredPath);
  }

#if DEBUG
  const string configurationName = "Debug";
#else
  const string configurationName = "Release";
#endif
  var defaultPath = Path.Combine(ResolveRepositoryRoot(), "Work", "bin", configurationName, "net10.0", "Work.dll");

  if (!File.Exists(defaultPath))
  {
    throw new FileNotFoundException(
      $"Die Baustein-Assembly wurde nicht gefunden: '{defaultPath}'. Bitte zuerst das Work-Projekt bauen " +
      "oder den Pfad über die Konfiguration 'BlockAssemblyPath' angeben.", defaultPath);
  }

  return defaultPath;
}

/// <summary>
/// Ermittelt das Verzeichnis mit den Workflow-JSON-Dateien (Standard: <c>Work/wwwroot/json</c>
/// im selben Repository). Über "WorkflowJsonDirectory" konfigurierbar.
/// </summary>
static string ResolveWorkflowJsonDirectory(IConfiguration configuration)
{
  var configuredPath = configuration["WorkflowJsonDirectory"];
  return string.IsNullOrWhiteSpace(configuredPath)
    ? Path.Combine(ResolveRepositoryRoot(), "Work", "wwwroot", "json")
    : Path.GetFullPath(configuredPath);
}
