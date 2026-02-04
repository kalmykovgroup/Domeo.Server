namespace Materials.API.ExternalServices;

public static class DependencyInjection
{
    public static IServiceCollection AddSupplierApiClient(this IServiceCollection services, IConfiguration configuration)
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
