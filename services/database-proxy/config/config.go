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

type Config struct {
	Proxy ProxyConfig `yaml:"proxy"`
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
