using Wallet.BuildingBlocks.Integration;
using Wallet.Domain.WalletAggregate;

namespace Wallet.Application.IntegrationEvents;

public record WalletTransactionCreatedIntegrationEvent(Guid WalletId, Transaction Transaction):IIntegrationEvent;