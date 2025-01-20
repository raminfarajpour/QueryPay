package tds

import (
	"encoding/binary"
)

type TabularResultParser struct{}

func (parser *TabularResultParser) Parse(packet []byte) PacketParsedResult {
	const rowCountToken = 0xFD
	const rowCountTokenInProc = 0xFF
	const rowCountTokenProc = 0xFE
	offset := 0

	for {
		if offset >= len(packet) {
			break
		}
		token := packet[offset]
		if token == rowCountToken {

			valid, command := isValidCommandResult(packet, offset)
			if !valid {
				offset++
				continue
			}
			offset += 5
			if len(packet) >= offset+8 {
				//log.Info().Msgf("rowCountToken %v", packet)
				rowCount := int64(binary.LittleEndian.Uint64(packet[offset : offset+8]))
				return PacketParsedResult{Keywords: []string{command}, RowCount: rowCount}
			}
			break
		}
		if token == rowCountTokenProc {
			/*status := packet[1]
			if status != 0x01 {
				offset++
				continue
			}*/
			valid, command := isValidCommandResult(packet, offset)
			if !valid {
				offset++
				continue
			}
			offset += 5
			if len(packet) >= offset+8 {
				//log.Info().Msgf("rowCountTokenProc %v", packet[offset:offset+8])
				rowCount := int64(binary.LittleEndian.Uint64(packet[offset : offset+8]))
				return PacketParsedResult{Keywords: []string{command}, RowCount: rowCount}
			}
			break
		}
		if token == rowCountTokenInProc {
			/*status := packet[1]
			if status != 0x01 {
				offset++
				continue
			}*/
			valid, command := isValidCommandResult(packet, offset)
			if !valid {
				offset++
				continue
			}
			offset += 5
			if len(packet) >= offset+8 {
				//log.Info().Msgf("rowCountTokenInProc %v", packet[offset:offset+8])
				rowCount := int64(binary.LittleEndian.Uint64(packet[offset : offset+8]))
				return PacketParsedResult{Keywords: []string{command}, RowCount: rowCount}
			}
			break
		}
		offset++
	}
	return PacketParsedResult{}

}

func isValidCommandResult(packet []byte, tokenIndex int) (bool, string) {
	commandMapping := map[uint16]string{
		0xC1: "SELECT",
		0xC3: "INSERT",
		0xC4: "DELETE",
		0xC5: "UPDATE",
	}
	curCmdBytes := packet[tokenIndex+3 : tokenIndex+5]
	curCmd := binary.LittleEndian.Uint16(curCmdBytes)
	command, exists := commandMapping[curCmd]
	if !exists {
		//log.Info().Msgf("invalid curr cmd %v", curCmdBytes)
	}
	return exists, command
}
