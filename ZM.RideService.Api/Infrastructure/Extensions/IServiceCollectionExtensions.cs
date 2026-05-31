using Carter;
using Microsoft.EntityFrameworkCore;
using ZM.RideService.Api.Persistance;
using ZM.RideService.Api.Persistance.Repositories;

namespace ZM.RideService.Api.Infrastructure.Extensions
{
    public static class IServiceCollectionExtensions
    {
        public static void RegisterServices(this IServiceCollection services, IConfiguration configuration)
        {
            RegisterEntityFramework(services);
            RegisterMediatR(services);
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
