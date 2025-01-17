using Wallet.BuildingBlocks.Domain;
using Wallet.Domain.WalletAggregate.Snapshots;

namespace Wallet.Domain.WalletAggregate;

public class Wallet : Aggregate<Wallet>

{
    public Money Balance { get; private set; }
    public Money OverUsedThreshold { get; private set; }
    public Owner Owner { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private readonly List<Transaction> _transactions = [];
    public IReadOnlyCollection<Transaction> Transactions => _transactions;

    protected override void Apply(Event<Wallet> e)
    {
        switch (e)
        {
            case WalletCreatedEvent createdEvent:
            {
                Balance = createdEvent.InitialBalance;
                OverUsedThreshold = createdEvent.OverUsedThreshold;
                Owner = createdEvent.Owner;
                CreatedAt = createdEvent.Timestamp;

                if (createdEvent.InitialBalance > (Money)0)
                {
                    var transactionInfo = new TransactionInfo(
                        Id.ToString(),
                        "Wallet Initial Balance");

                    var transaction = new Transaction(transactionInfo,
                        createdEvent.InitialBalance,
                        (Money)0L,
                        createdEvent.InitialBalance,
                        TransactionDirection.Increase);

                    _transactions.Add(transaction);
                }

                break;
            }
            case WalletDepositedEvent depositedEvent:
            {
                var balanceBefore = Balance;
                Balance += depositedEvent.Amount;

                var transaction = new Transaction(depositedEvent.TransactionInfo,
                    depositedEvent.Amount,
                    balanceBefore,
                    Balance,
                    TransactionDirection.Increase);

                _transactions.Add(transaction);

                break;
            }
            case WalletWithdrawalEvent withdrawalEvent:
            {
                if (Balance + OverUsedThreshold < withdrawalEvent.Amount)
                    throw new InsufficientBalanceException();

                var balanceBefore = Balance;
                Balance -= withdrawalEvent.Amount;

                var transaction = new Transaction(withdrawalEvent.TransactionInfo,
                    withdrawalEvent.Amount,
                    balanceBefore,
                    Balance,
                    TransactionDirection.Decrease);

                _transactions.Add(transaction);
                break;
            }
        }
    }

    protected override void Apply(Snapshot<Wallet> snapshot)
    {
        switch (snapshot)
        {
            case WalletSnapshot walletSnapshot:
            {
                Balance = walletSnapshot.Balance;
                OverUsedThreshold = walletSnapshot.OverUsedThreshold;
                Owner = walletSnapshot.Owner;
                CreatedAt = walletSnapshot.CreatedAt;
                break;
            }
        }
    }

    public void Create(Money initialBalance, Money overUsedThreshold, Owner owner)
    {
        Apply(new WalletCreatedEvent(owner, initialBalance, overUsedThreshold));
    }

    public void Deposit(Money amount, TransactionInfo transactionInfo)
    {
        Apply(new WalletDepositedEvent(amount, transactionInfo));
    }

    public void Withdraw(Money amount, TransactionInfo transactionInfo)
    {
        Apply(new WalletWithdrawalEvent(amount, transactionInfo));
    }

    public void ClearTransaction() => _transactions.Clear();
}