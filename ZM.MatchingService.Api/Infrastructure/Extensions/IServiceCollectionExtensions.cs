using MassTransit;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using ZM.MatchingService.Api.Application.Cache;
using ZM.MatchingService.Api.Infrastructure.Caches;
using ZM.MatchingService.Api.Persistence;

namespace ZM.MatchingService.Api.Infrastructure.Extensions
{
    public static class IServiceCollectionExtensions
    {
        public static void RegisterServices(this IServiceCollection services, IConfiguration configuration)
        {
            RegisterEntityFramework(services);
            RegisterMediatR(services);
            RegisterMassTransit(services, configuration);
            RegisterRedis(services, configuration);
            RegisterCaches(services);
            RegisterDateTimeProviders(services);
            RegisterQueries(services);
        }

        private static void RegisterEntityFramework(IServiceCollection services)
        {
            services.AddDbContext<MatchingDbContext>(options => options
               .UseInMemoryDatabase("AvailableDriver")
               .UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll), ServiceLifetime.Scoped);
        }

        private static void RegisterMediatR(IServiceCollection services)
        {
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(MatchingDbContext).Assembly));
        }

        private static void RegisterMassTransit(IServiceCollection services, IConfiguration configuration)
        {
            services.AddMassTransit(busConfigurator =>
            {
                busConfigurator.SetKebabCaseEndpointNameFormatter();

                busConfigurator.UsingRabbitMq((context, configurator) =>
                {
                    configurator.Host(new Uri(configuration["MessageBroker:Host"]!), h =>
                    {
                        h.Username(configuration["MessageBroker:Username"]!);
                        h.Password(configuration["MessageBroker:Password"]!);
                    });

                    configurator.ConfigureEndpoints(context);
                });
            });
        }

        private static void RegisterRedis(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IConnectionMultiplexer>(_ => 
                ConnectionMultiplexer.Connect(
                    configuration.GetConnectionString("Redis")!));
        }

        private static void RegisterCaches(IServiceCollection services)
        {
            services.AddScoped<IAvailableDriverCache, RedisDriverMatchingCache>();
        }

        private static void RegisterDateTimeProviders(IServiceCollection services)
        {
            services.Scan(scan => scan
                .FromAssemblyOf<MatchingDbContext>()
                .AddClasses(classes => classes.Where(c => c.Name.EndsWith("DateTimeProvider")))
                .AsMatchingInterface()
                .WithScopedLifetime());
        }

        private static void RegisterQueries(IServiceCollection services)
        {
            services.Scan(scan => scan
                .FromAssemblyOf<MatchingDbContext>()
                .AddClasses(classes => classes.Where(c => c.Name.EndsWith("Query")))
                .AsMatchingInterface()
                .WithScopedLifetime());
        }
    }
}
