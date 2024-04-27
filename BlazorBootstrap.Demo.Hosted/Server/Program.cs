var builder = WebApplication.CreateBuilder(args);
{
    // DI
    builder.Services.AddHttpClient();
    builder.Services.AddBlazorBootstrap();
    builder.Services.AddDemoServices();

    builder.Services.AddControllersWithViews();
    builder.Services.AddRazorPages();
}

var app = builder.Build();
{
    if (app.Environment.IsDevelopment())
    {
        app.UseWebAssemblyDebugging();
    }
    else
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();

    app.UseBlazorFrameworkFiles();
    app.UseStaticFiles();

    app.UseRouting();

    app.MapRazorPages();
    app.MapControllers();
    app.MapFallbackToPage("/_Host");

    app.Run();
}
