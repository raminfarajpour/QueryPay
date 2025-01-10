package config

import (
	"fmt"
	"gopkg.in/yaml.v3"
	"os"
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

type Config struct {
	Proxy    ProxyConfig    `yaml:"proxy"`
	Redis    RedisConfig    `yaml:"redis"`
	RabbitMq RabbitMqConfig `yaml:"rabbitmq"`
}

func LoadConfig(filename string) (*Config, error) {
	file, err := os.Open(filename)
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
