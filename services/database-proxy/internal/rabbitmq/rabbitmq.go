package rabbitmq

import (
	"context"
	"github.com/rabbitmq/amqp091-go"
	"github.com/rs/zerolog/log"
)

const (
	exchange        = "query_pay_usage_events_exchange"
	routingKey      = "ue"
	queue           = "query_pay_usage_events"
	retryExchange   = "retry_query_pay_usage_events_exchange"
	retryRoutingKey = "rue"
	retryQueue      = "retry_query_pay_usage_events"
)

type Publisher struct {
	conn    *amqp091.Connection
	channel *amqp091.Channel
}

func NewPublisher(rabbitMQURL string) (*Publisher, error) {
	connection, err := amqp091.Dial(rabbitMQURL)
	if err != nil {
		return nil, err
	}
	channel, err := connection.Channel()
	if err != nil {
		connection.Close()
		return nil, err
	}
	declareExchanges(channel)

	return &Publisher{
		conn:    connection,
		channel: channel,
	}, nil
}
func (publisher *Publisher) Publish(context context.Context, exchange string, routingKey string, data []byte) error {
	err := publisher.channel.PublishWithContext(
		context,
		exchange,
		routingKey,
		false,
		false,
		amqp091.Publishing{
			ContentType: "application/json",
			Body:        data,
		},
	)
	if err != nil {
		log.Fatal().Msgf("error in publishing message: %v", err)
		return err
	}
	return nil
}
func (publisher *Publisher) Close() {
	if err := publisher.channel.Close(); err != nil {
		log.Printf("error in closing channel: %v", err)
	}
	if err := publisher.conn.Close(); err != nil {
		log.Printf("error in closing connection: %v", err)
	}
}

func declareExchanges(channel *amqp091.Channel) {
	err := channel.ExchangeDeclare(
		exchange,
		"direct",
		true,
		false,
		false,
		false,
		nil,
	)
	if err != nil {
		log.Fatal().Msgf("Failed to declare main exchange: %v", err)
	}

	err = channel.ExchangeDeclare(
		retryExchange,
		"direct",
		true,
		false,
		false,
		false,
		nil,
	)
	if err != nil {
		log.Fatal().Msgf("Failed to declare retry exchange: %v", err)
	}

	_, err = channel.QueueDeclare(
		queue,
		true,
		false,
		false,
		false,
		amqp091.Table{
			"x-dead-letter-exchange":    retryExchange,
			"x-dead-letter-routing-key": retryRoutingKey,
		},
	)
	if err != nil {
		log.Fatal().Msgf("Failed to declare main queue: %v", err)
	}

	err = channel.QueueBind(
		queue,
		routingKey,
		exchange,
		false,
		nil,
	)
	if err != nil {
		log.Fatal().Msgf("Failed to bind main queue: %v", err)
	}

	_, err = channel.QueueDeclare(
		retryQueue, // Queue name
		true,       // Durable
		false,      // Auto-deleted
		false,      // Exclusive
		false,      // No-wait
		amqp091.Table{
			"x-message-ttl":             int32(120000),
			"x-dead-letter-exchange":    exchange,
			"x-dead-letter-routing-key": routingKey,
		},
	)
	if err != nil {
		log.Fatal().Msgf("Failed to declare retry queue: %v", err)
	}

	err = channel.QueueBind(
		retryQueue,
		retryRoutingKey,
		retryExchange,
		false,
		nil,
	)
	if err != nil {
		log.Fatal().Msgf("Failed to bind retry queue: %v", err)
	}
}
