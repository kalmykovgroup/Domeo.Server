using Clients.Abstractions.Repositories;
using Clients.API.Infrastructure.Persistence;
using Clients.API.Infrastructure.Persistence.Repositories;
using Domeo.Shared.Application;
using FluentValidation;
using MediatR;

namespace Clients.API.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }

    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ClientsDbContext>());

        return services;
    }
}
