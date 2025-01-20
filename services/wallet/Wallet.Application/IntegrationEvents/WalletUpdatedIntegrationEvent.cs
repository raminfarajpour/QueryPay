using Wallet.BuildingBlocks.Integration;
using Wallet.Domain.WalletAggregate;

namespace Wallet.Application.IntegrationEvents;

public record WalletUpdatedIntegrationEvent(Guid WalletId,Money Balance,Money OverUsedThreshold,Owner Owner):IIntegrationEvent;