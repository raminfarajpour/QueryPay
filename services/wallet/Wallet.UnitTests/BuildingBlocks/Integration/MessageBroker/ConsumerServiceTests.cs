using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RabbitMQ.Client;
using Wallet.BuildingBlocks.Integration.MessageBroker;
using Xunit;

namespace Wallet.UnitTests.BuildingBlocks.Integration.MessageBroker;

public class ConsumerServiceTests
{
    private readonly Mock<IRabbitMqConnectionManager> _mockRabbitMqConnectionManager;
    private readonly Mock<ILogger<ConsumerService>> _mockLogger;
    private readonly Mock<IChannel> _mockChannel;       
    private readonly ConsumerService _consumerService;

    public ConsumerServiceTests()
    {
        _mockRabbitMqConnectionManager = new Mock<IRabbitMqConnectionManager>();
        _mockLogger = new Mock<ILogger<ConsumerService>>();
        _mockChannel = new Mock<IChannel>();
        _mockRabbitMqConnectionManager.Setup(cm => cm.GetChannel()).Returns(_mockChannel.Object);
        _consumerService = new ConsumerService(_mockRabbitMqConnectionManager.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task ConsumeAsync_ShouldAddConsumerSuccessfully()
    {
        // Arrange
        var mockHandler = new Mock<IMessageHandler>();
        var mockQueueSetting = new RabbitMqQueueSetting
        {
            Name = "test-queue",
            PrefetchCount = 10
        };
        mockHandler.Setup(h => h.QueueSetting).Returns(mockQueueSetting);
        var cancellationToken = CancellationToken.None;

        var mockChannel = new Mock<IChannel>();
        _mockRabbitMqConnectionManager.Setup(m => m.GetChannel()).Returns(mockChannel.Object);
        _mockRabbitMqConnectionManager.Setup(m => m.InitialChannelAsync(cancellationToken)).Returns(Task.CompletedTask);

        // Act
        await _consumerService.ConsumeAsync(mockHandler.Object, cancellationToken);

        // Assert
        _mockRabbitMqConnectionManager.Verify(m => m.InitialChannelAsync(cancellationToken), Times.Once);
        mockChannel.Verify(c => c.BasicQosAsync(0, mockQueueSetting.PrefetchCount, true, CancellationToken.None),
            Times.Once);
    }

    [Fact]
    public async Task ConsumeAsync_ShouldHandleConsumerCreationFailure()
    {
        // Arrange
        var mockHandler = new Mock<IMessageHandler>();
        var mockQueueSetting = new RabbitMqQueueSetting
        {
            Name = "faulty-queue",
            PrefetchCount = 10
        };
        mockHandler.Setup(h => h.QueueSetting).Returns(mockQueueSetting);
        var cancellationToken = CancellationToken.None;

        _mockRabbitMqConnectionManager.Setup(m => m.InitialChannelAsync(cancellationToken))
            .ThrowsAsync(new Exception("Channel Initialization Failed"));

        // Act
        Func<Task> act = async () => await _consumerService.ConsumeAsync(mockHandler.Object, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Channel Initialization Failed");
        
    }
    
}