using Microsoft.Extensions.DependencyInjection;
using Wallet.Application.Commands.CreateWallet;
using Wallet.BuildingBlocks.Domain;
using Wallet.BuildingBlocks.Integration;
using Wallet.Domain.WalletAggregate.Snapshots;

namespace Wallet.Application;

public static class ApplicationInstaller
{
    public static void ConfigureApplication(this IServiceCollection services)
    {
        services.AddMediatR(c=>c.RegisterServicesFromAssembly(typeof(CreateWalletCommand).Assembly));

    }
}