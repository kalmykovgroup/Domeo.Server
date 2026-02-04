using Auth.Abstractions.Repositories;
using Auth.API.Infrastructure.Persistence;
using Auth.API.Infrastructure.Persistence.Repositories;
using Domeo.Shared.Kernel.Application.Abstractions;

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
