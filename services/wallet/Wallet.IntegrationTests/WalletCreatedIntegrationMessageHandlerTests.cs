using System.Text;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Newtonsoft.Json;
using Wallet.Application.IntegrationEvents;
using Wallet.BuildingBlocks.Integration;
using Wallet.BuildingBlocks.Integration.MessageBroker;
using Wallet.Domain.WalletAggregate;
using Wallet.Domain.WalletAggregate.Snapshots;
using Wallet.Infrastructure.Integration;
using Wallet.Infrastructure.Persistence.EventStore.Entities;
using Wallet.IntegrationTests.Fixtures;
using Wallet.ReadModel.Repositories;
using Xunit;

namespace Wallet.IntegrationTests;

public class WalletCreatedIntegrationMessageHandlerTests(
    ReadModelFixture readModelFixture) : IClassFixture<ReadModelFixture>
{

    [Fact]
    public async Task HandleAsync_ShouldConsumeMessagesAndSaveSuccessfully()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();


        var eventBusSetting = readModelFixture.ServiceProvider.GetRequiredService<IOptions<RabbitMqSetting>>();
        var walletReadModelRepository = readModelFixture.ServiceProvider.GetRequiredService<IWalletReadModelRepository>();
        var handlers = readModelFixture.ServiceProvider.GetServices<IMessageHandler>();
        var consumer = readModelFixture.ServiceProvider.GetRequiredService<IConsumerService>();
        foreach (var handler in handlers)
        {
            await consumer.ConsumeAsync(handler,CancellationToken.None);
        }
        
        var sampleEvent = new WalletCreatedIntegrationEvent(aggregateId, new Money(100), new Money(500),
            new Owner(123123, "123123"), DateTimeOffset.UtcNow);
        await PublishAsync(sampleEvent,
          ( (EventBusSetting) eventBusSetting.Value).WalletIntegrationEventsExchange.WalletCreatedEventQueue.RoutingKey, CancellationToken.None);

        await Task.Delay(2000).WaitAsync(CancellationToken.None);
        
        var wallet = await walletReadModelRepository.GetByIdAsync(aggregateId,CancellationToken.None);
        wallet.Should().NotBeNull();
        wallet.Balance.Should().Be(100);
    }

    private async Task PublishAsync(IIntegrationEvent @event, string routingKey, CancellationToken cancellationToken)
    {
        var message = JsonConvert.SerializeObject(@event);
        var data = Encoding.UTF8.GetBytes(message);

        var producerService = readModelFixture.ServiceProvider.GetRequiredService<IProducerService>();
        var eventBusSetting = readModelFixture.ServiceProvider.GetRequiredService<IOptions<RabbitMqSetting>>();

        await producerService.ProduceAsync(data, ((EventBusSetting)eventBusSetting.Value).WalletIntegrationEventsExchange.Name,
            routingKey, persist: true,
            cancellationToken: cancellationToken);
    }
}