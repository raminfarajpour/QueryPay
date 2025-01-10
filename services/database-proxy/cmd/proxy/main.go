package main

import (
	"github.com/go-redis/redis/v8"
	"github.com/raminfarajpour/database-proxy/config"
	"github.com/raminfarajpour/database-proxy/internal/outbox"
	"github.com/raminfarajpour/database-proxy/internal/proxy"
	"github.com/rs/zerolog/log"
	"os"
	"path/filepath"
)

func main() {

	workingDir, err := os.Getwd()

	configFileName := "config/config.yaml"

	configPath := filepath.Join(workingDir, configFileName)
	config, err := config.LoadConfig(configPath)
	if err != nil {
		log.Fatal().Msgf("failed to load config file %w \n", err)
	}

	redisClient := redis.NewClient(&redis.Options{
		Addr:     config.Redis.Address,
		Password: config.Redis.Password,
		DB:       config.Redis.DB,
	})

	outbox := outbox.NewOutbox(redisClient)

	proxyServer, err := proxy.NewDatabaseProxyServer(config.Proxy.ListenPort, config.Proxy.DestinationHost, config.Proxy.DestinationPort, outbox)

	if err != nil {
		log.Fatal().Msgf("fail to create proxy server. error: %v\n", err)
	}

	log.Info().Msg("Starting proxy server...")
	if err := proxyServer.Listen(); err != nil {
		log.Fatal().Msgf("Proxy server failed: %v\n", err)
	}

}
