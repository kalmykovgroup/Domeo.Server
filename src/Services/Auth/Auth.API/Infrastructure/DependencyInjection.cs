using Auth.Domain.Repositories;
using Auth.Infrastructure.Persistence;
using Domeo.Shared.Application;

namespace Auth.API.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services;
    }

    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AuthDbContext>());

        return services;
    }
}
