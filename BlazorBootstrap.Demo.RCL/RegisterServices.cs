namespace BlazorBootstrap.Demo.RCL;

public static class RegisterServices
{
    public static IServiceCollection AddDemoServices(this IServiceCollection services)
    {
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<ICustomerService, CustomerService>();

        return services;
    }
}
