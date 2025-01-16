using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Wallet.BuildingBlocks.Integration.MessageBroker;
using Xunit;

namespace Wallet.UnitTests.BuildingBlocks.Integration.MessageBroker;

public class RabbitMqConnectionFactoryTests
{
    private readonly Mock<ILogger<RabbitMqConnectionFactory>> _mockLogger;
    private readonly Mock<IOptions<RabbitMqSetting>> _mockOptions;
    private readonly RabbitMqSetting _validSettings;
    private readonly RabbitMqConnectionFactory _factory;

    public RabbitMqConnectionFactoryTests()
    {
        _mockLogger = new Mock<ILogger<RabbitMqConnectionFactory>>();
        _mockOptions = new Mock<IOptions<RabbitMqSetting>>();

        _validSettings = new TestRabbitMqSetting()
        {
            ConnectionSetting = new RabbitMqConnectionSetting
            {
                Username = "testUser",
                Password = "testPass",
                HostName = "localhost",
                Port = 5672,
                VirtualHost = "/"
            },
            TestExchange =  new TestRabbitMqSetting.TestRabbitMqExchangeSetting{ Name = "test-exchange", Type = "direct", Durable = true }
            
        };

        _mockOptions.Setup(o => o.Value).Returns(_validSettings);

        _factory = new RabbitMqConnectionFactory(_mockLogger.Object, _mockOptions.Object);
    }
    

    [Fact]
    public async Task CreateConnectionAsync_Should_Throw_Exception_When_Invalid_Host()
    {
        // Arrange
        _validSettings.ConnectionSetting.HostName = "invalid-host";
        _mockOptions.Setup(o => o.Value).Returns(_validSettings);
        var cancellationToken = CancellationToken.None;

        // Act
        Func<Task> act = async () => await _factory.CreateConnectionAsync(cancellationToken);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public void Constructor_Should_Throw_ArgumentNullException_When_Options_Is_Null()
    {
        // Arrange
        IOptions<RabbitMqSetting> nullOptions = null!;

        // Act
        Action act = () => new RabbitMqConnectionFactory(_mockLogger.Object, nullOptions);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("rabbitOptions");
    }

    [Fact]
    public async Task CreateConnectionAsync_Should_Throw_Exception_When_Username_Is_Null()
    {
        // Arrange
        _validSettings.ConnectionSetting.Username = null!;
        _mockOptions.Setup(o => o.Value).Returns(_validSettings);
        var cancellationToken = CancellationToken.None;

        // Act
        Func<Task> act = async () => await _factory.CreateConnectionAsync(cancellationToken);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task CreateConnectionAsync_Should_Throw_Exception_When_Password_Is_Null()
    {
        // Arrange
        _validSettings.ConnectionSetting.Password = null!;
        _mockOptions.Setup(o => o.Value).Returns(_validSettings);
        var cancellationToken = CancellationToken.None;

        // Act
        Func<Task> act = async () => await _factory.CreateConnectionAsync(cancellationToken);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task CreateConnectionAsync_Should_Throw_Exception_When_Port_Is_Invalid()
    {
        // Arrange
        _validSettings.ConnectionSetting.Port = -1;
        _mockOptions.Setup(o => o.Value).Returns(_validSettings);
        var cancellationToken = CancellationToken.None;

        // Act
        Func<Task> act = async () => await _factory.CreateConnectionAsync(cancellationToken);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task CreateConnectionAsync_Should_Throw_Exception_When_VirtualHost_Is_Null()
    {
        // Arrange
        _validSettings.ConnectionSetting.VirtualHost = null!;
        _mockOptions.Setup(o => o.Value).Returns(_validSettings);
        var cancellationToken = CancellationToken.None;

        // Act
        Func<Task> act = async () => await _factory.CreateConnectionAsync(cancellationToken);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }
}


