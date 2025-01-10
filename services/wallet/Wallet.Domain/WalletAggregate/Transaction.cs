using Wallet.BuildingBlocks.Domain;

namespace Wallet.Domain.WalletAggregate;

public class Transaction:Entity<long>
{
    private Transaction(){}

    public Transaction(TransactionInfo transactionInfo, Money amount, Money balanceBefore, Money balanceAfter,TransactionDirection transactionDirection)
    {
        TransactionInfo = transactionInfo;
        Amount = amount;
        BalanceBefore = balanceBefore;
        BalanceAfter = balanceAfter;
        Direction = transactionDirection;
    }
    public TransactionInfo TransactionInfo { get; init; }
    public Money Amount { get; init; }
    public Money BalanceBefore { get; init; }
    public Money BalanceAfter { get; init; }
    public TransactionDirection Direction { get; init; }
}