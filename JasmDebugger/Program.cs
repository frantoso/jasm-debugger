using JasmDebugger.Client;
using JasmDebugger.Components;
using JasmDebugger.Model;
using JasmDebugger.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddSignalR();
builder.Services.AddSingleton(_ => new SvgHubClient("http://localhost:5076"));
builder.Services.AddHostedService<TcpBackgroundService>();
builder.Services.AddSingleton<TcpClientContainer>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(JasmDebugger.Client._Imports).Assembly);

app.MapHub<SvgHub>("/fsmHub");

app.Run();
