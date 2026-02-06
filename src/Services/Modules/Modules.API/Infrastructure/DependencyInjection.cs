using Domeo.Shared.Application;
using FluentValidation;
using MediatR;
using Modules.Domain.Repositories;
using Modules.Infrastructure.Persistence;
using Modules.Infrastructure.Persistence.Repositories;
using Modules.Infrastructure.Storage;

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
        services.AddScoped<IStorageConnectionRepository, StorageConnectionRepository>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ModulesDbContext>());

        services.AddHttpContextAccessor();
        services.AddScoped<FileStorageResolver>();

        return services;
    }
}
