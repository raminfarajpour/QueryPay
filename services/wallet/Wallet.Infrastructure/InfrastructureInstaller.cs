using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wallet.Infrastructure.Persistence.EventStore;
using Wallet.Infrastructure.Persistence.EventStore.Repositories;

namespace Wallet.Infrastructure;

public static class InfrastructureInstaller
{
    public static void ConfigureInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<EventStoreContext>(options=>options.UseNpgsql(configuration.GetConnectionString("EventStore")));
        services.AddScoped<IEventStore, EventStore>();
    }

}