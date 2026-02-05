using Domeo.Shared.Application;
using FluentValidation;
using MediatR;
using Modules.Abstractions.Repositories;
using Modules.API.Infrastructure.Persistence;
using Modules.API.Infrastructure.Persistence.Repositories;

namespace Modules.API.Infrastructure;

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
        services.AddScoped<IModuleCategoryRepository, ModuleCategoryRepository>();
        services.AddScoped<IAssemblyRepository, AssemblyRepository>();
        services.AddScoped<IComponentRepository, ComponentRepository>();
        services.AddScoped<IAssemblyPartRepository, AssemblyPartRepository>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ModulesDbContext>());

        return services;
    }
}
