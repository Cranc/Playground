using System.Reflection;
using WorkflowEngine.DependencyInjection;
using WorkflowEngine.Editor.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var blockAssembly = Assembly.LoadFrom(ResolveBlockAssemblyPath(builder.Configuration));
builder.Services.AddWorkflowEngine(blockAssembly);

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

  var repositoryRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
#if DEBUG
  const string configurationName = "Debug";
#else
  const string configurationName = "Release";
#endif
  var defaultPath = Path.Combine(repositoryRoot, "Work", "bin", configurationName, "net10.0", "Work.dll");

  if (!File.Exists(defaultPath))
  {
    throw new FileNotFoundException(
      $"Die Baustein-Assembly wurde nicht gefunden: '{defaultPath}'. Bitte zuerst das Work-Projekt bauen " +
      "oder den Pfad über die Konfiguration 'BlockAssemblyPath' angeben.", defaultPath);
  }

  return defaultPath;
}
