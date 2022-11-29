using BlazorBootstrap;
using Microsoft.AspNetCore.ResponseCompression;

var builder = WebApplication.CreateBuilder(args);
{
    // Add services to the container.
    builder.Services.AddBlazorBootstrap();

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
    app.MapFallbackToFile("/_Host");

    app.Run();
}