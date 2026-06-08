using Carter;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using ZM.RideService.Api.Persistence;
using ZM.RideService.Api.Persistence.Repositories;

namespace ZM.RideService.Api.Infrastructure.Extensions
{
    public static class IServiceCollectionExtensions
    {
        public static void RegisterServices(this IServiceCollection services, IConfiguration configuration)
        {
            RegisterEntityFramework(services);
            RegisterMediatR(services);
            RegisterMassTransit(services, configuration);
            RegisterCarter(services);
            RegisterUnitOfWorks(services);
            RegisterDateTimeProviders(services);
            RegisterQueries(services);
            RegisterRepositories(services);
        }

        private static void RegisterEntityFramework(IServiceCollection services)
        {
            services.AddDbContext<RideDbContext>(options => options
               .UseInMemoryDatabase("Ride")
               .UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll), ServiceLifetime.Scoped);
        }

        private static void RegisterMediatR(IServiceCollection services)
        {
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(RideRepository).Assembly));
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

        private static void RegisterCarter(IServiceCollection services)
        {
            services.AddCarter();
        }

        private static void RegisterUnitOfWorks(IServiceCollection services)
        {
            services.Scan(scan => scan
                .FromAssemblyOf<RideRepository>()
                .AddClasses(classes => classes.Where(c => c.Name.EndsWith("UnitOfWork")))
                .AsMatchingInterface()
                .WithScopedLifetime());
        }

        private static void RegisterDateTimeProviders(IServiceCollection services)
        {
            services.Scan(scan => scan
                .FromAssemblyOf<RideRepository>()
                .AddClasses(classes => classes.Where(c => c.Name.EndsWith("DateTimeProvider")))
                .AsMatchingInterface()
                .WithScopedLifetime());
        }

        private static void RegisterQueries(IServiceCollection services)
        {
            services.Scan(scan => scan
                .FromAssemblyOf<RideRepository>()
                .AddClasses(classes => classes.Where(c => c.Name.EndsWith("Query")))
                .AsMatchingInterface()
                .WithScopedLifetime());
        }

        private static void RegisterRepositories(IServiceCollection services)
        {
            services.Scan(scan => scan
                .FromAssemblyOf<RideRepository>()
                .AddClasses(classes => classes.Where(c => c.Name.EndsWith("Repository")))
                .AsMatchingInterface()
                .WithScopedLifetime());
        }
    }
}
