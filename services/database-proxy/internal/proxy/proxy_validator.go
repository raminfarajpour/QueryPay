package proxy

import (
	"fmt"
	"net"
)

func isValidPort(port int) bool {
	return port > 0 && port <= 65535
}

func isValidHost(host string) bool {
	if net.ParseIP(host) != nil {
		return true
	}
	_, err := net.LookupHost(host)
	return err == nil
}

func (dbProxy *DatabaseProxyServer) Validate() error {

	if !isValidPort(dbProxy.ListenPort) {
		return fmt.Errorf("invalid listen port: %d", dbProxy.ListenPort)
	}

	if !isValidHost(dbProxy.DestinationHost) {
		return fmt.Errorf("invalid destination host: %s", dbProxy.DestinationHost)
	}

	if !isValidPort(dbProxy.DestinationPort) {
		return fmt.Errorf("invalid destination port: %d", dbProxy.DestinationPort)
	}

	return nil
}
