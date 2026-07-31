using Microsoft.EntityFrameworkCore;
using MassTransit;
using ZM.NotificationService.Api.Infrastructure.Consumers;
using ZM.NotificationService.Api.Persistence;

namespace ZM.NotificationService.Api.Infrastructure.Extensions
{
    public static class IServiceCollectionExtensions
    {
        public static void RegisterServices(this IServiceCollection services, IConfiguration configuration)
        {
            RegisterEntityFramework(services);
            RegisterMediatR(services);
            RegisterMassTransit(services, configuration);
            RegisterTemplates(services);
            RegisterSenders(services);
        }

        private static void RegisterEntityFramework(IServiceCollection services)
        {
            services.AddDbContext<NotificationDbContext>(options => options
               .UseInMemoryDatabase("Notification")
               .UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll), ServiceLifetime.Scoped);
        }

        private static void RegisterMediatR(IServiceCollection services)
        {
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IServiceCollectionExtensions).Assembly));
        }

        private static void RegisterMassTransit(IServiceCollection services, IConfiguration configuration)
        {
            services.AddMassTransit(busConfigurator =>
            {
                busConfigurator.SetKebabCaseEndpointNameFormatter();

                busConfigurator.AddConsumer<DriverAssignedConsumer>();
                busConfigurator.AddConsumer<PaymentReceiptConsumer>();
                busConfigurator.AddConsumer<RideCompletedConsumer>();

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

        private static void RegisterTemplates(IServiceCollection services)
        {
            services.Scan(scan => scan
                .FromAssemblyOf<AssemblyMarker>()
                .AddClasses(classes => classes.Where(c => c.Name.EndsWith("Templates")))
                .AsMatchingInterface()
                .WithScopedLifetime());
        }

        private static void RegisterSenders(IServiceCollection services)
        {
            services.Scan(scan => scan
                .FromAssemblyOf<AssemblyMarker>()
                .AddClasses(classes => classes.Where(c => c.Name.EndsWith("Sender")))
                .AsMatchingInterface()
                .WithScopedLifetime());
        }
    }
}
