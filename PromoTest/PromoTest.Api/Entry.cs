using Microsoft.EntityFrameworkCore;
using PromoTest.Api.Persistence;
using PromoTest.Api.Services;
using PromoTest.Api.Services.Interfaces;

namespace PromoTest.Api;

public static class Entry
{
    public static IServiceCollection AddTaskApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<EfContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<ITaskService, TaskService>();

        services.AddSingleton<RabbitMqService>();

        services.AddSingleton<ITaskEventPublisher>(provider =>
            provider.GetRequiredService<RabbitMqService>());

        services.AddHostedService(provider =>
            provider.GetRequiredService<RabbitMqService>());

        return services;
    }
}