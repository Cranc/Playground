using Work.Components;
using TicketPlanner.Services;
using WorkflowEngine.DependencyInjection;
using Work.Workflows.Steps;
using Work.Workflows.DriverTour.Steps;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDevExpressBlazor();
builder.Services.AddWorkflowEngine();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// TicketPlanner services
builder.Services
  .AddScoped<ITicketService, DummyTicketService>()
  .AddScoped<ITicketPlanStorageService, TicketPlanStorageService>()
  .AddScoped<CreateUserStep>()
  .AddScoped<FinishStep>()
  .AddScoped<InitializeTourStep>()
  .AddScoped<CompleteShipmentStep>()
  .AddScoped<CompleteBulkShipmentsStep>()
  .AddScoped<CompleteStopStep>()
  .AddScoped<AdvanceStopStep>()
  .AddLocalStorageServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
  app.UseExceptionHandler("/Error", createScopeForErrors: true);
  // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
  app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
