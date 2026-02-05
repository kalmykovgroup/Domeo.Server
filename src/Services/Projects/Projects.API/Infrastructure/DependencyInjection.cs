using Domeo.Shared.Application;
using FluentValidation;
using MediatR;
using Projects.Domain.Repositories;
using Projects.Infrastructure.Persistence;
using Projects.Infrastructure.Persistence.Repositories;

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
