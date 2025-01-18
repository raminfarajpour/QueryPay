package config

import (
	"fmt"
	"github.com/rs/zerolog/log"
	"gopkg.in/yaml.v3"
	"os"
	"path/filepath"
)

type ProxyConfig struct {
	ListenPort      int    `yaml:"listen_port"`
	DestinationHost string `yaml:"destination_host"`
	DestinationPort int    `yaml:"destination_port"`
}

type RedisConfig struct {
	Address  string `yaml:"address"`
	Password string `yaml:"password"`
	DB       int    `yaml:"db"`
}

type RabbitMqConfig struct {
	Host     string `yaml:"host"`
	Port     int    `yaml:"port"`
	Username string `yaml:"username"`
	Password string `yaml:"password"`
}

type BillingServiceConfig struct {
	Host string `yaml:"host"`
	Port int    `yaml:"port"`
}

type Config struct {
	Proxy          ProxyConfig          `yaml:"proxy"`
	Redis          RedisConfig          `yaml:"redis"`
	RabbitMq       RabbitMqConfig       `yaml:"rabbitmq"`
	BillingService BillingServiceConfig `yaml:"billing_service"`
}

func GetConfigFile() string {
	configPath := os.Getenv("CONFIG_FILE")
	if configPath != "" {
		return configPath
	}

	workingDir, err := os.Getwd()
	if err != nil {
		log.Fatal().Msg("error in getting working directory")
	}
	configFileName := "config/config.yaml"

	configPath = filepath.Join(workingDir, configFileName)
	return configPath
}

func LoadConfig() (*Config, error) {
	configFile := GetConfigFile()
	file, err := os.Open(configFile)
	if err != nil {
		return nil, fmt.Errorf("failed to open config file: %w", err)
	}
	defer file.Close()

	config := &Config{}
	decoder := yaml.NewDecoder(file)
	if err := decoder.Decode(config); err != nil {
		return nil, fmt.Errorf("failed to decode config file: %w", err)
	}

	return config, nil
}
