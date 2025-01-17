using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Wallet.BuildingBlocks.Domain;
using Wallet.Domain.WalletAggregate.Snapshots;
using Wallet.Infrastructure.Persistence.EventStore;
using Wallet.Infrastructure.Persistence.EventStore.Repositories;
using Xunit;

namespace Wallet.IntegrationTests.Fixtures;

public class DatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer;
    public EventStoreContext Context { get; private set; }
    public IServiceProvider ServiceProvider { get; private set; }
    public EventStore EventStore { get; private set; }

    public DatabaseFixture()
    {
        _postgreSqlContainer = new PostgreSqlBuilder()
            .WithImage("postgres:14")
            .WithHostname("localhost")
            .WithDatabase("test_wallet_db")
            .WithUsername("postgres")
            .WithPassword("test_user_p@ssword")
            .WithPortBinding("5490")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();

        var services = new ServiceCollection();
        services.AddScoped<ISnapshotFactory<Domain.WalletAggregate.Wallet>, WalletSnapshotFactory>();
        ServiceProvider = services.BuildServiceProvider();

        var options = new DbContextOptionsBuilder<EventStoreContext>()
            .UseNpgsql(_postgreSqlContainer.GetConnectionString())
            .Options;

        Context = new EventStoreContext(options);
        await Context.Database.MigrateAsync();

        EventStore = new EventStore(Context, ServiceProvider);
    }

    public async Task DisposeAsync()
    {
        await Context.Database.EnsureDeletedAsync();
        await _postgreSqlContainer.StopAsync();
        Context.Dispose();
    }
}