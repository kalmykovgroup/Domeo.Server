using Domeo.Shared.Kernel.Application.Abstractions;
using Domeo.Shared.Kernel.Application.Behaviors;
using FluentValidation;
using MediatR;
using Projects.Abstractions.Repositories;
using Projects.API.Infrastructure.Persistence;
using Projects.API.Infrastructure.Persistence.Repositories;

namespace Projects.API.Infrastructure;

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
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ProjectsDbContext>());

        return services;
    }
}
