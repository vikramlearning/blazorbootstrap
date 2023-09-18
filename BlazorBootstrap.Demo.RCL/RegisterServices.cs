namespace BlazorBootstrap.Demo.RCL;

public static class RegisterServices
{
    public static IServiceCollection RegisterDemoService(this IServiceCollection services)
    {
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<ICustomerService, CustomerService>();

        return services;
    }
}
