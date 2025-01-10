package tds

import "fmt"

type PacketType int

const (
	SqlBatch      PacketType = 1 // SQL Batch
	Rpc           PacketType = 3 // RPC
	TabularResult PacketType = 4 // Tabular result
	SQLBatch48    PacketType = 48
	Response      PacketType = 32
)

func GetPacketType(buffer []byte) (PacketType, error) {
	if len(buffer) < 1 {
		return 0, fmt.Errorf("buffer too short to determine packet type")
	}

	packetType := PacketType(buffer[0])

	switch packetType {
	case SqlBatch, Response, Rpc, TabularResult, SQLBatch48:
		return packetType, nil
	default:
		return 0, fmt.Errorf("unknown packet type: %d", packetType)
	}
}
