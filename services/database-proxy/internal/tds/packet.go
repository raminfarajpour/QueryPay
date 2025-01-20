package tds

type Packet struct {
	Buffer []byte
	Type   PacketType
	Parser PacketParser
}

func NewPacket(packet []byte) (*Packet, error) {
	packetType, err := GetPacketType(packet)
	if err != nil {
		return nil, err
	}

	packetParser := GetPacketParser(packetType)

	return &Packet{
		Buffer: packet,
		Type:   packetType,
		Parser: packetParser,
	}, nil
}
func (packet *Packet) Parse() PacketParsedResult {

	if packet.Parser == nil {
		return PacketParsedResult{}
	}
	return packet.Parser.Parse(packet.Buffer)
}
