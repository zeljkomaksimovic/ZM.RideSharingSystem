using Microsoft.EntityFrameworkCore;
using Carter;
using MassTransit;
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
            RegisterCarter(services);
            RegisterUnitOfWorks(services);
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

        private static void RegisterCarter(IServiceCollection services)
        {
            services.AddCarter();
        }

        private static void RegisterUnitOfWorks(IServiceCollection services)
        {
            services.Scan(scan => scan
                .FromAssemblyOf<MatchingDbContext>()
                .AddClasses(classes => classes.Where(c => c.Name.EndsWith("UnitOfWork")))
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
