using boltalka.Application.Abstractions.Services;
using boltalka.Application.Abstractions.Storage;
using boltalka.Infrastructure.Database.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace boltalka.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureApplication(this IServiceCollection services)
    {
        services.AddScoped<IFileStorage, LocalFileStorage>();
            
        return services;
    }

    
}