using BlazorBootstrap;
using BlazorBootstrap.Demo.Server;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpClient();
builder.Services.AddBlazorBootstrap();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
