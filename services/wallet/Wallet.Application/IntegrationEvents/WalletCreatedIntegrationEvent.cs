using Wallet.BuildingBlocks.Integration;
using Wallet.Domain.WalletAggregate;

namespace Wallet.Application.IntegrationEvents;

public record WalletCreatedIntegrationEvent(Guid WalletId,Money Balance,Money OverUsedThreshold,Owner Owner,DateTimeOffset CreatedAt):IIntegrationEvent;