using BlazorBootstrap; // Add this line

...

public static async Task Main(string[] args)
{
    var builder = WebAssemblyHostBuilder.CreateDefault(args);
    builder.RootComponents.Add<App>("#app");

    builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

    // Inject services           
    builder.Services.AddBlazorBootstrap(); // Add this line

    await builder.Build().RunAsync();
}

...