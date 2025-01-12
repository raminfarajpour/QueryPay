using MediatR;
using Wallet.Domain.WalletAggregate;
using Wallet.Infrastructure.Persistence.EventStore.Repositories;

namespace Wallet.Application.Commands.CreateWallet;

public class CreateWalletCommandHandler(IEventStore  eventStore) : IRequestHandler<CreateWalletCommand>
{
    public async Task Handle(CreateWalletCommand command, CancellationToken cancellationToken)
    {
        var wallet = new Domain.WalletAggregate.Wallet();
        wallet.Create(
            initialBalance:new Money(command.InitialBalance),
            overUsedThreshold:new Money(command.OverUsedThreshold),
            new Owner(command.OwnerUserId, command.OwnerMobile)
        );
        
        await eventStore.PersistAsync(wallet,cancellationToken);
    }
}