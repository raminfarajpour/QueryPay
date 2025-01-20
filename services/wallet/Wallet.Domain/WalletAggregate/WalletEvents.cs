using Wallet.BuildingBlocks.Domain;

namespace Wallet.Domain.WalletAggregate;

public record WalletCreatedEvent(Owner Owner,Money InitialBalance,Money OverUsedThreshold):Event<Wallet>;
public record WalletDepositedEvent(Money Amount,TransactionInfo TransactionInfo):Event<Wallet>;
public record WalletWithdrawalEvent(Money Amount,TransactionInfo TransactionInfo):Event<Wallet>;