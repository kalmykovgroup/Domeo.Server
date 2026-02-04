using FluentValidation;
using Materials.Abstractions.ExternalServices;
using Materials.API.Infrastructure.ExternalServices;
using MediatR;

namespace Materials.API.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }

    public static IServiceCollection AddSupplierApiClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var baseUrl = configuration["SupplierApi:BaseUrl"] ?? "http://localhost:5102";
        var timeoutSeconds = configuration.GetValue<int>("SupplierApi:TimeoutSeconds", 30);

        services.AddHttpClient<ISupplierApiClient, SupplierApiClient>(client =>
        {
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        return services;
    }
}
