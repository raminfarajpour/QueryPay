using MediatR;
using Wallet.Domain.WalletAggregate;
using Wallet.Infrastructure.Persistence.EventStore.Repositories;

namespace Wallet.Application.Commands.CreateWallet;

public class CreateWalletCommandHandler(IEventStore eventStore) : IRequestHandler<CreateWalletCommand>
{
    public async Task Handle(CreateWalletCommand command, CancellationToken cancellationToken)
    {
        var wallet =
            await eventStore.RehydrateAsync<Domain.WalletAggregate.Wallet>(
                 Guid.Parse("bf015df2-af66-4a72-b15e-8cebcd9bac7e"), cancellationToken: cancellationToken);

        // var wallet = new Domain.WalletAggregate.Wallet();
        // wallet.Create(
        //     initialBalance: new Money(command.InitialBalance),
        //     overUsedThreshold: new Money(command.OverUsedThreshold),
        //     new Owner(command.OwnerUserId, command.OwnerMobile)
        // );
        for (int i = 0; i < 150; i++)
        {
            wallet.Deposit(new Money(3000L),new TransactionInfo("asd-asdasd","asdasda","test description"));
        }
        
        await eventStore.PersistAsync(wallet, cancellationToken);
        
        // var cuurentWallet =
        //     await eventStore.RehydrateAsync<Domain.WalletAggregate.Wallet>(
        //         wallet.Id, cancellationToken: cancellationToken);
    }
}