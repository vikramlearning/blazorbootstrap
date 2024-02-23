var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
if (builder.Environment.IsDevelopment())
    builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:5031/") });
else
    builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://demos.blazorbootstrap.com/") });

builder.Services.AddBlazorBootstrap();
builder.Services.AddDemoServices();

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
