using Microsoft.Extensions.DependencyInjection;
using Wallet.BuildingBlocks.Domain;
using Wallet.Domain.WalletAggregate.Snapshots;

namespace Wallet.Domain;

public static class DomainInstaller
{
    public static void Install(this IServiceCollection services)
    {
        services.AddScoped<ISnapshotFactory<WalletAggregate.Wallet>,WalletSnapshotFactory>();
    }
}