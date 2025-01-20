using FluentAssertions;
using Moq;
using RabbitMQ.Client;
using Wallet.BuildingBlocks.Integration.MessageBroker;
using Xunit;

namespace Wallet.UnitTests.BuildingBlocks.Integration.MessageBroker
{
    public class ProducerServiceTests
    {
        private readonly Mock<IRabbitMqConnectionManager> _mockConnectionManager;
        private readonly Mock<IChannel> _mockChannel;
        private readonly ProducerService _producerService;

        public ProducerServiceTests()
        {
            _mockConnectionManager = new Mock<IRabbitMqConnectionManager>();
            _mockChannel = new Mock<IChannel>();
            _mockConnectionManager.Setup(cm => cm.GetChannel()).Returns(_mockChannel.Object);

            _producerService = new ProducerService(_mockConnectionManager.Object);
        }

        [Fact]
        public async Task ProduceAsync_ShouldCallInitialChannelAsync()
        {
            // Arrange
            var data = new byte[] { 0x01, 0x02 };
            var exchange = "test-exchange";
            var routingKey = "test-routing-key";

            // Act
            await _producerService.ProduceAsync(data, exchange, routingKey);

            // Assert
            _mockConnectionManager.Verify(cm => cm.InitialChannelAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task ProduceAsync_Should_CallBasicPublishAsync()
        {
            // Arrange
            var data = new byte[] { 0x03, 0x04 };
            var exchange = "another-exchange";
            var routingKey = "another-routing-key";
            bool persist = true;

            _mockChannel
                .Setup(ch => ch.BasicPublishAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<BasicProperties>(),
                    It.IsAny<ReadOnlyMemory<byte>>(),
                    It.IsAny<CancellationToken>()))
                .Returns(ValueTask.CompletedTask);

            // Act
            await _producerService.ProduceAsync(data, exchange, routingKey, persist);

            // Assert
            _mockChannel.Verify(ch => ch.BasicPublishAsync(
                exchange,
                routingKey,
                true,
                It.IsAny<BasicProperties>(),
                data,
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ProduceAsync_ShouldThrowArgumentNullException_WhenDataIsNull()
        {
            // Arrange
            byte[] data = null;
            var exchange = "test-exchange";
            var routingKey = "test-routing-key";

            // Act
            var act = async () => await _producerService.ProduceAsync(data, exchange, routingKey);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithParameterName("data");
        }

        

        [Fact]
        public async Task ProduceAsync_ShouldHandleInitialChannelAsyncException()
        {
            // Arrange
            var data = new byte[] { 0x09 };
            var exchange = "exception-exchange";
            var routingKey = "exception-routing-key";

            _mockConnectionManager
                .Setup(cm => cm.InitialChannelAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Initialization failed."));

            // Act
            Func<Task> act = async () => await _producerService.ProduceAsync(data, exchange, routingKey);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Initialization failed.");

            _mockChannel.Verify(ch => ch.BasicPublishAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<BasicProperties>(),
                It.IsAny<ReadOnlyMemory<byte>>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ProduceAsync_ShouldHandleBasicPublishAsyncException()
        {
            // Arrange
            var data = new byte[] { 0x0A };
            var exchange = "publish-exchange";
            var routingKey = "publish-routing-key";

            _mockChannel
                .Setup(ch => ch.BasicPublishAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<BasicProperties>(),
                    It.IsAny<ReadOnlyMemory<byte>>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Publish failed."));

            // Act
            Func<Task> act = async () => await _producerService.ProduceAsync(data, exchange, routingKey);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Publish failed.");

            _mockConnectionManager.Verify(cm => cm.InitialChannelAsync(It.IsAny<CancellationToken>()), Times.Once);
            _mockChannel.Verify(ch => ch.BasicPublishAsync(
                exchange,
                routingKey,
                true,
                It.IsAny<BasicProperties>(),
                data,
                It.IsAny<CancellationToken>()), Times.Once);
        }

        
    }
}
