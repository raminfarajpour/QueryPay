using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions; 
using RabbitMQ.Client.Events;
using Wallet.BuildingBlocks.Extensions;
using Wallet.BuildingBlocks.Integration;
using Wallet.BuildingBlocks.Integration.MessageBroker;
using Xunit;

namespace Wallet.UnitTests.BuildingBlocks.Integration.MessageBroker
{
    public class MessageHandlerTests
    {
        private readonly ILogger<MessageHandler<TestMessage>> _logger;
        private readonly TestMessageHandler _messageHandler;

        public MessageHandlerTests()
        {
            // Use NullLogger to avoid mocking ILogger<T>
            _logger = NullLogger<MessageHandler<TestMessage>>.Instance;
            _messageHandler = new TestMessageHandler(_logger);
        }

        [Fact]
        public async Task HandleAsync_ShouldAcknowledgeMessage()
        {
            // Arrange
            bool ackCalled = false;
            BasicDeliverEventArgs? receivedArgs = null;
            string? receivedQueueName = null;
            CancellationToken? receivedCancellationToken = null;

            // Define the Ack action
            Func<BasicDeliverEventArgs, string, CancellationToken, Task> ackAction = (args, queueName, ct) =>
            {
                ackCalled = true;
                receivedArgs = args;
                receivedQueueName = queueName;
                receivedCancellationToken = ct;
                return Task.CompletedTask;
            };

            bool nackCalled = false;

            Task NackAction(BasicDeliverEventArgs args, string queueName, bool requeue, CancellationToken ct)
            {
                nackCalled = true;
                return Task.CompletedTask;
            }

            var validMessage = new TestMessage("test-message");
            var mockBody = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(validMessage.ToJson()));
            var args = new BasicDeliverEventArgs(
                consumerTag: "123",
                deliveryTag: 123,
                redelivered: false,
                exchange: "test-exchange",
                routingKey: "test-routing-key",
                 null!,
                body: mockBody
            );

            // Act
            await _messageHandler.HandleAsync(null, args, ackAction, NackAction, CancellationToken.None);

            // Assert
            ackCalled.Should().BeTrue("AckAction should be called for valid messages");
            receivedArgs.Should().BeSameAs(args);
            receivedQueueName.Should().Be(_messageHandler.QueueSetting.Name);
            receivedCancellationToken.Should().Be(CancellationToken.None);

            nackCalled.Should().BeFalse("NackAction should not be called for valid messages");
        }

        [Fact]
        public async Task HandleAsync_ShouldNAcknowledgeOnError()
        {
            // Arrange
            bool ackCalled = false;

            Func<BasicDeliverEventArgs, string, CancellationToken, Task> ackAction = (args, queueName, ct) =>
            {
                ackCalled = true;
                return Task.CompletedTask;
            };

            bool nackCalled = false;
            BasicDeliverEventArgs? receivedArgs = null;
            string? receivedQueueName = null;
            bool receivedRequeue = false;
            CancellationToken? receivedCancellationToken = null;

            Func<BasicDeliverEventArgs, string, bool, CancellationToken, Task> nackAction = (args, queueName, requeue, ct) =>
            {
                nackCalled = true;
                receivedArgs = args;
                receivedQueueName = queueName;
                receivedRequeue = requeue;
                receivedCancellationToken = ct;
                return Task.CompletedTask;
            };

            var invalidMessage = new TestMessage("invalid-message");
            var mockBody = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(invalidMessage.ToJson()));
            var args = new BasicDeliverEventArgs(
                consumerTag: "123",
                deliveryTag: 123,
                redelivered: false,
                exchange: "test-exchange",
                routingKey: "test-routing-key",
                 null!,
                body: mockBody
            );

            // Act
            var act = async () =>
                await _messageHandler.HandleAsync(null, args, ackAction, nackAction, CancellationToken.None);

            // Assert
            await act.Should().NotThrowAsync("HandleAsync should handle exceptions internally and not propagate them");

            ackCalled.Should().BeFalse("AckAction should not be called when an error occurs");
            nackCalled.Should().BeTrue("NackAction should be called when an error occurs");
            receivedArgs.Should().BeSameAs(args);
            receivedQueueName.Should().Be(_messageHandler.QueueSetting.Name);
            receivedRequeue.Should().BeFalse("Requeue should be false on error");
            receivedCancellationToken.Should().Be(CancellationToken.None);
        }

        private class TestMessage : IIntegrationEvent
        {
            public string Value { get; set; }

            public TestMessage(string value)
            {
                Value = value;
            }
        }

        private class TestMessageHandler(ILogger<MessageHandler<TestMessage>> logger)
            : MessageHandler<TestMessage>(logger)
        {
            public override RabbitMqQueueSetting QueueSetting { get; set; } =
                new RabbitMqQueueSetting { Name = "test-queue" };

            protected override Task HandleAsync(TestMessage? message, CancellationToken cancellationToken)
            {
                ArgumentNullException.ThrowIfNull(message);

                if (message.Value == "invalid-message")
                {
                    throw new Exception("Invalid message");
                }

                return Task.CompletedTask;
            }
        }
    }
}
