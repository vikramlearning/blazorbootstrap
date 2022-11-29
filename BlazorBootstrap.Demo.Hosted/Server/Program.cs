using BlazorBootstrap;
using BlazorBootstrap.Demo;
using Microsoft.AspNetCore.ResponseCompression;

var builder = WebApplication.CreateBuilder(args);
{
    // DI
    builder.Services.AddHttpClient();
    builder.Services.AddBlazorBootstrap();
    builder.Services.AddScoped<IEmployeeService, EmployeeService>();
    builder.Services.AddScoped<ICustomerService, CustomerService>();

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