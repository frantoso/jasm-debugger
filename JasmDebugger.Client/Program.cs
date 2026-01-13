using JasmDebugger.Client;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddSingleton(_ => new SvgHubClient(builder.HostEnvironment.BaseAddress));

var app = builder.Build();

await app.RunAsync();
