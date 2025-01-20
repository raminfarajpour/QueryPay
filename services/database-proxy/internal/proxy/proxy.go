package proxy

import (
	"context"
	"fmt"
	"github.com/raminfarajpour/database-proxy/internal/events"
	"github.com/raminfarajpour/database-proxy/internal/outbox"
	"github.com/raminfarajpour/database-proxy/internal/tds"
	"github.com/rs/zerolog/log"
	"io"
	"net"
	"sync"
)

type DatabaseProxyServer struct {
	ListenPort      int
	DestinationHost string
	DestinationPort int
	Outbox          *outbox.Outbox
}

func NewDatabaseProxyServer(listenPort int, destinationHost string, destinationPort int, outbox *outbox.Outbox) (*DatabaseProxyServer, error) {
	proxyServer := DatabaseProxyServer{
		listenPort,
		destinationHost,
		destinationPort,
		outbox,
	}
	err := proxyServer.Validate()
	if err != nil {
		return nil, err
	}
	return &proxyServer, nil

}

func (dbProxy *DatabaseProxyServer) Listen() error {

	const protocol = "tcp"
	listenPort := fmt.Sprintf(":%d", dbProxy.ListenPort)
	proxyListener, err := net.Listen(protocol, listenPort)
	if err != nil {
		return err
	}

	defer proxyListener.Close()

	log.Info().Msgf("listening on port %d", dbProxy.ListenPort)

	for {
		connection, err := proxyListener.Accept()
		if err != nil {
			log.Error().Msgf("error accepting connection: %v", err)
			continue
		}

		go func(conn net.Conn) {
			defer func() {
				if r := recover(); r != nil {
					log.Error().Msgf("Recovered from panic: %v", r)
				}
				conn.Close()
			}()

			dbProxy.handleConnection(conn)
		}(connection)
	}

	return nil
}

func (dbProxy *DatabaseProxyServer) handleConnection(upstream net.Conn) {

	dbAddress := fmt.Sprintf("%s:%d", dbProxy.DestinationHost, dbProxy.DestinationPort)
	downstream, err := net.Dial("tcp", dbAddress)
	if err != nil {
		log.Fatal().Msgf("error in connecting to server %s", dbAddress)
	}

	defer downstream.Close()
	var wg sync.WaitGroup
	wg.Add(2)

	go func() {
		defer wg.Done()
		dbProxy.forwardClientToServerAndInspect(upstream, downstream)
	}()

	go func() {
		defer wg.Done()
		dbProxy.forwardServerToClientAndInspect(downstream, upstream)
	}()

	wg.Wait()
	log.Info().Msg("all packets handled successfully")

}

func (dbProxy *DatabaseProxyServer) forwardClientToServerAndInspect(client, server net.Conn) {
	buf := make([]byte, 4096)
	for {
		n, err := client.Read(buf)
		if err != nil {
			if err != io.EOF {
				log.Printf("[%s] error reading client: %v\n", err)
			}
			break
		}

		dbProxy.analyzePacket(buf[:n])

		_, err = server.Write(buf[:n])
		if err != nil {
			log.Printf("[%s] error writing to server: %v\n", err)
			break
		}
	}
}
func (dbProxy *DatabaseProxyServer) forwardServerToClientAndInspect(server, client net.Conn) {
	//buf := make([]byte, 4096)
	for {
		header := make([]byte, 8)
		_, err := io.ReadFull(server, header)
		if err != nil {
			if err != io.EOF {
				log.Printf("error reading TDS header from server: %v\n", err)
			}
			break
		}
		packetLength := int(header[2])<<8 | int(header[3])

		packetBody := make([]byte, packetLength-8)
		_, err = io.ReadFull(server, packetBody)
		if err != nil {
			log.Printf("error reading TDS packet body from server: %v\n", err)
			break
		}

		fullPacket := append(header, packetBody...)

		dbProxy.analyzePacket(fullPacket)

		_, err = client.Write(fullPacket)
		if err != nil {
			log.Printf("error writing to client: %v\n", err)
			break
		}
		/*n, err := io.ReadFull(server, buf)
		if err != nil {
			if err != io.EOF {
				log.Printf("[%s] error reading server: %v\n", err)
			}
			break
		}
		dbProxy.analyzePacket(buf[:n])

		_, err = client.Write(buf[:n])
		if err != nil {
			log.Printf("[%s] error writing to client: %v\n", err)
			break
		}*/
	}
}

func (dbProxy *DatabaseProxyServer) analyzePacket(data []byte) {
	if len(data) < 1 {
		return
	}

	packet, err := tds.NewPacket(data)
	if err != nil {
		log.Info().Msgf("error getting packet info %v\n", err)
		return
	}

	result := packet.Parse()

	if (result.Keywords != nil && len(result.Keywords) > 0) || result.RowCount > 0 {

		//billingProvider := billing.BillingServiceProvider{}

		//isBalanceSufficient, err := billingProvider.CheckUserBalance(111222333, result.Keywords, result.RowCount)
		//if err != nil {
		//	log.Fatal().Msgf("error checking user balance %v\n", err)
		//}
		//if isBalanceSufficient == false {
		//	log.Fatal().Msg("user balance is not valid ")
		//}

		event, err := events.NewEvent(result)
		if err != nil {
			log.Fatal().Msgf("error in creating event for %v with error %v", result, err)
			return
		}
		ctx := context.Background()
		dbProxy.Outbox.WriteEvent(ctx, event)
	}

	log.Info().Msgf("status %#X type: %#X parsed packet %v\n", data[1], data[0], result)

}
