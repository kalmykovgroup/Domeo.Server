using Audit.Abstractions.Repositories;
using Audit.API.Infrastructure.Persistence;
using Audit.API.Infrastructure.Persistence.Repositories;
using Domeo.Shared.Kernel.Application.Abstractions;
using Domeo.Shared.Kernel.Application.Behaviors;
using FluentValidation;
using MediatR;

namespace Audit.API.Infrastructure;

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
        services.AddScoped<IApplicationLogRepository, ApplicationLogRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<ILoginSessionRepository, LoginSessionRepository>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AuditDbContext>());

        return services;
    }
}
