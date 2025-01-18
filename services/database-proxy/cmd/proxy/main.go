package main

import (
	"context"
	"fmt"
	"github.com/go-redis/redis/v8"
	"github.com/joho/godotenv"
	"github.com/raminfarajpour/database-proxy/config"
	"github.com/raminfarajpour/database-proxy/internal/outbox"
	"github.com/raminfarajpour/database-proxy/internal/proxy"
	"github.com/raminfarajpour/database-proxy/internal/rabbitmq"
	"github.com/rs/zerolog/log"
	"os"
	"os/signal"
	"syscall"
)

func main() {

	godotenv.Load()

	config, err := config.LoadConfig()
	if err != nil {
		log.Fatal().Msgf("failed to load config file %w \n", err)
	}

	redisClient := redis.NewClient(&redis.Options{
		Addr:     config.Redis.Address,
		Password: config.Redis.Password,
		DB:       config.Redis.DB,
	})

	outboxHandler := outbox.NewOutbox(redisClient)

	proxyServer, err := proxy.NewDatabaseProxyServer(config.Proxy.ListenPort, config.Proxy.DestinationHost, config.Proxy.DestinationPort, outboxHandler)

	if err != nil {
		log.Fatal().Msgf("fail to create proxy server. error: %v\n", err)
	}

	rabbitMQURL := fmt.Sprintf("amqp://%s:%s@%s:%d/",
		config.RabbitMq.Username,
		config.RabbitMq.Password,
		config.RabbitMq.Host,
		config.RabbitMq.Port,
	)
	rabbitMqPublisher, err := rabbitmq.NewPublisher(rabbitMQURL)
	if err != nil {
		log.Fatal().Msgf("error in creating publisher %v", rabbitMQURL)
	}

	publishWorker := outbox.NewPublishWorker(outboxHandler, rabbitMqPublisher)

	ctx, cancel := context.WithCancel(context.Background())
	defer cancel()

	// Capture OS signals for shutdown
	signalChan := make(chan os.Signal, 1)
	signal.Notify(signalChan, syscall.SIGINT, syscall.SIGTERM)

	go func() {
		log.Info().Msg("Starting proxy server...")
		if err := proxyServer.Listen(); err != nil {
			log.Fatal().Msgf("Proxy server failed: %v\n", err)
		}
	}()

	go func() {
		log.Info().Msg("Starting publish worker...")
		publishWorker.Start(ctx)
	}()

	// Wait for shutdown signal
	<-signalChan
	rabbitMqPublisher.Close()
	log.Info().Msg("Shutting down gracefully...")
	cancel()

}
