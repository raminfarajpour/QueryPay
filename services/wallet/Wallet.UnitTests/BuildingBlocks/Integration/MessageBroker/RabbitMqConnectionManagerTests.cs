// RabbitMqConnectionManagerTests.cs

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RabbitMQ.Client;
using Wallet.BuildingBlocks.Integration.MessageBroker;
using Xunit;

namespace Wallet.UnitTests.BuildingBlocks.Integration.MessageBroker;

public class RabbitMqConnectionManagerTests
{
    private readonly Mock<IOptions<TestRabbitMqSetting>> _mockOptions;
    private readonly Mock<IRabbitMqConnectionFactory<TestRabbitMqSetting>> _mockConnectionFactory;
    private readonly Mock<IConnection> _mockConnection;
    private readonly Mock<IChannel> _mockChannel;
    private readonly RabbitMqConnectionManager<TestRabbitMqSetting> _connectionManager;

    public RabbitMqConnectionManagerTests()
    {
        _mockOptions = new Mock<IOptions<TestRabbitMqSetting>>();
        _mockConnection = new Mock<IConnection>();
        _mockChannel = new Mock<IChannel>();

        var testSetting = new TestRabbitMqSetting
        {
            ConnectionSetting = new RabbitMqConnectionSetting
            {
                Username = "testUser",
                Password = "testPass",
                HostName = "localhost",
                Port = 5672,
                VirtualHost = "/"
            },
            TestExchange =
                new TestRabbitMqSetting.TestRabbitMqExchangeSetting
                {
                    Name = "test-exchange",
                    Type = "direct",
                    Durable = true,
                    AutoDelete = false,
                    TestQueue = new RabbitMqQueueSetting
                    {
                        Name = "test-queue",
                        RoutingKey = "test-routing-key",
                        Durable = true,
                        Exclusive = false,
                        AutoDelete = false,
                        Ttl = 60,
                        RetryQueue = new RabbitMqQueueSetting
                        {
                            Name = "retry-test-queue",
                            RoutingKey = "retry-routing-key",
                            Durable = true,
                            Exclusive = false,
                            AutoDelete = false,
                            Ttl = 120
                        }
                    }
                }
        };

        _mockOptions.Setup(o => o.Value).Returns(testSetting);

        _mockConnectionFactory = new Mock<IRabbitMqConnectionFactory<TestRabbitMqSetting>>();

        // _mockConnectionFactory =
        //     new Mock<IRabbitMqConnectionFactory>(new Mock<ILogger<RabbitMqConnectionFactory>>().Object,
        //         _mockOptions.Object);

        _mockConnectionFactory.Setup(c => c.CreateConnectionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult((_mockConnection.Object, testSetting.GetExchanges())));
        _mockConnection
            .Setup(conn => conn.CreateChannelAsync(It.IsAny<CreateChannelOptions?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_mockChannel.Object);

        _connectionManager = new RabbitMqConnectionManager<TestRabbitMqSetting>(_mockConnectionFactory.Object,
            new Mock<ILogger<RabbitMqConnectionManager<TestRabbitMqSetting>>>().Object);
        // _connectionManager = new Mock<IRabbitMqConnectionManager>(_mockConnectionFactory.Object,
        //     new Mock<ILogger<RabbitMqConnectionManager>>().Object);
    }

    [Fact]
    public async Task InitialChannelAsync_ShouldCreateConnectionAndChannelAndDeclareExchangesAndQueues()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        ConfigureMockChannel();

        // Act
        await _connectionManager.InitialChannelAsync(cancellationToken);

        // Assert
        _mockConnection.Verify(conn => conn.CreateChannelAsync(null, cancellationToken), Times.Once);
        _mockChannel.Verify(c => c.ExchangeDeclareAsync(It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<IDictionary<string, object?>>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()),
            Times.Exactly(2));


        _mockChannel.Verify(c => c.QueueDeclareAsync(
            It.IsAny<string>(),
            It.IsAny<bool>(),
            It.IsAny<bool>(),
            It.IsAny<bool>(),
            It.IsAny<IDictionary<string, object?>>(),
            It.IsAny<bool>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()), Times.Exactly(2));

        _mockChannel.Verify(c => c.QueueBindAsync(It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IDictionary<string, object?>>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task InitialChannelAsync_Should_Not_Create_New_Channel_If_Already_Initialized()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        ConfigureMockChannel();

        // Act
        await _connectionManager.InitialChannelAsync(cancellationToken);
        await _connectionManager.InitialChannelAsync(cancellationToken);

        // Assert
        _mockConnection.Verify(conn => conn.CreateChannelAsync(null, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task GetChannel_Should_Return_Channel_When_Initialized()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        ConfigureMockChannel();

        await _connectionManager.InitialChannelAsync(cancellationToken);

        // Act
        var channel = _connectionManager.GetChannel();

        // Assert
        channel.Should().Be(_mockChannel.Object);
    }

    [Fact]
    public void GetChannel_Should_Throw_InvalidOperationException_If_Not_Initialized()
    {
        // Act
        var act = () => _connectionManager.GetChannel();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Channel has not been initialized. Call InitialChannelAsync first.");
    }

    [Fact]
    public async Task InitialChannelAsync_Should_Throw_Exception_When_CreateConnectionAsync_Fails()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;


        _mockConnection
            .Setup(conn => conn.CreateChannelAsync(It.IsAny<CreateChannelOptions?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Connection failed"));

        // Act
        Func<Task> act = async () => await _connectionManager.InitialChannelAsync(cancellationToken);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Connection failed");
    }

    [Fact]
    public async Task InitialChannelAsync_Should_Throw_Exception_When_CreateChannelAsync_Fails()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        _mockConnection
            .Setup(conn => conn.CreateChannelAsync(It.IsAny<CreateChannelOptions?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Connection failed"));


        // Act
        Func<Task> act = async () => await _connectionManager.InitialChannelAsync(cancellationToken);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Connection failed");
    }

    private void ConfigureMockChannel()
    {
        _mockChannel.Setup(c => c.ExchangeDeclareAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<IDictionary<string, object?>>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockChannel.Setup(c => c.QueueDeclareAsync(
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<IDictionary<string, object?>>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(new QueueDeclareOk("test-queue", UInt32.MinValue, UInt32.MinValue)));

        _mockChannel.Setup(c => c.QueueBindAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IDictionary<string, object?>>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }
}

public class TestRabbitMqSetting : RabbitMqSetting
{
    public TestRabbitMqExchangeSetting TestExchange { get; set; }

    public override List<RabbitMqExchangeSetting> GetExchanges() => [TestExchange];

    public class TestRabbitMqExchangeSetting : RabbitMqExchangeSetting
    {
        public RabbitMqQueueSetting TestQueue { get; set; }

        public override List<RabbitMqQueueSetting> GetQueues() =>
        [
            TestQueue
        ];
    }
}