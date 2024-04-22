var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<BlazorBootstrap.Demo.WebAssembly.App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Register services
builder.Services.AddBlazorBootstrap();
builder.Services.AddDemoServices();

await builder.Build().RunAsync();
