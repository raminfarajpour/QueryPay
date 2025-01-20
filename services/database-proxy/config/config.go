package config

import (
	"fmt"
	"github.com/rs/zerolog/log"
	"github.com/spf13/viper"
	"os"
	"path/filepath"
	"strings"
)

type ProxyConfig struct {
	ListenPort      int    `mapstructure:"listen_port"`
	DestinationHost string `mapstructure:"destination_host"`
	DestinationPort int    `mapstructure:"destination_port"`
}

type RedisConfig struct {
	Address  string `mapstructure:"address"`
	Password string `mapstructure:"password"`
	DB       int    `mapstructure:"db"`
}

type RabbitMqConfig struct {
	Host     string `mapstructure:"host"`
	Port     int    `mapstructure:"port"`
	Username string `mapstructure:"username"`
	Password string `mapstructure:"password"`
}

type BillingServiceConfig struct {
	Host string `mapstructure:"host"`
	Port int    `mapstructure:"port"`
}

type CertificateConfig struct {
	File string `mapstructure:"file"`
}

type Config struct {
	Proxy          ProxyConfig          `mapstructure:"proxy"`
	Redis          RedisConfig          `mapstructure:"redis"`
	RabbitMq       RabbitMqConfig       `mapstructure:"rabbitmq"`
	BillingService BillingServiceConfig `mapstructure:"billing_service"`
	Certificate    CertificateConfig    `mapstructure:"certificate"`
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

	workingDir, err := os.Getwd()
	if err != nil {
		log.Fatal().Msg("error in getting working directory")
	}
	configFileName := "config"
	configPath := filepath.Join(workingDir, "config")

	viper.SetConfigName(configFileName)
	viper.SetConfigType("yaml")
	viper.AddConfigPath(configPath)

	viper.AutomaticEnv()
	viper.SetEnvKeyReplacer(strings.NewReplacer(".", "_"))

	if err := viper.ReadInConfig(); err != nil {
		return nil, fmt.Errorf("error reading config file: %w", err)
	}

	var cfg Config
	if err := viper.Unmarshal(&cfg); err != nil {
		return nil, fmt.Errorf("unable to decode into struct: %w", err)
	}

	return &cfg, nil
}
