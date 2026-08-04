using Carter;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Quartz;
using ZM.RideService.Api.Application.DomainEventCollector;
using ZM.RideService.Api.Application.Pricing;
using ZM.RideService.Api.Infrastructure.BackgroundJobs;
using ZM.RideService.Api.Infrastructure.Consumers;
using ZM.RideService.Api.Infrastructure.Pricing;
using ZM.RideService.Api.Infrastructure.Sagas.RideCreated;
using ZM.RideService.Api.Persistence;
using ZM.RideService.Api.Persistence.DomainEventCollector;
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
            RegisterQuartz(services);
            RegisterCarter(services);
            RegisterDomainEventCollector(services);
            RegisterPricingServices(services);
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

                busConfigurator.AddConsumer<AssignDriverConsumer>();
                busConfigurator.AddConsumer<CompleteRideConsumer>();
                busConfigurator.AddConsumer<RideStartedConsumer>();

                busConfigurator.AddSagaStateMachine<RideCreatedSaga, RideCreatedSagaData>()
                .InMemoryRepository();

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

        private static void RegisterQuartz(IServiceCollection services)
        {
            services.AddQuartz(configure =>
            {
                var jobKey = new JobKey(nameof(ProcessOutboxMessagesJob));

                configure
                .AddJob<ProcessOutboxMessagesJob>(jobKey, job => job.WithIdentity(jobKey))
                .AddTrigger(
                    options =>
                        options.ForJob(jobKey)
                            .WithSimpleSchedule(
                            schedule =>
                                schedule.WithIntervalInSeconds(5)
                                        .RepeatForever()));
            });

            services.AddQuartzHostedService();
        }

        private static void RegisterCarter(IServiceCollection services)
        {
            services.AddCarter();
        }

        private static void RegisterDomainEventCollector(IServiceCollection services)
        {
            services.AddScoped<IDomainEventCollector, DomainEventCollector>();
        }

        private static void RegisterPricingServices(IServiceCollection services)
        {
            services.AddScoped<IFareCalculator, FareCalculator>();
            services.AddScoped<IFareEstimator, FareEstimator>();
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
