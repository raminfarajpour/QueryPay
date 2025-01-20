package tds

type PacketParsedResult struct {
	Keywords []string
	RowCount int64
}
type PacketParser interface {
	Parse([]byte) PacketParsedResult
}

var PacketParserCollection = map[PacketType]PacketParser{
	//SqlBatch:      &SQLBatchParser{},
	TabularResult: &TabularResultParser{},
	//SQLBatch48:    &SQLBatchParser{},
}

func GetPacketParser(packetType PacketType) PacketParser {
	if cmd, exists := PacketParserCollection[packetType]; exists {
		return cmd
	}
	return nil
}
