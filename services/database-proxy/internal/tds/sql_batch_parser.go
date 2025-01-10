package tds

import (
	"encoding/binary"
	"fmt"
	"regexp"
	"strings"
	"unicode/utf16"
)

type SQLBatchParser struct{}

func (parser *SQLBatchParser) Parse(packet []byte) PacketParsedResult {
	const queryStartOffset = 30
	sqlTextBytes := packet[queryStartOffset:len(packet)]

	sqlText, err := DecodeUTF16(sqlTextBytes)
	//log.Info().Msg(sqlText)
	if err != nil {
		return PacketParsedResult{}
	}
	return PacketParsedResult{Keywords: extractKeyWordsFromSqlText(sqlText)}

}

func extractKeyWordsFromSqlText(sqlText string) []string {
	sqlKeywords := []string{"SELECT", "UPDATE", "DELETE", "INSERT", "CREATE"}
	keywordPattern := `(?i)\b(` + strings.Join(sqlKeywords, "|") + `)\b`
	regex := regexp.MustCompile(keywordPattern)

	return regex.FindAllString(sqlText, -1)

}
func DecodeUTF16(data []byte) (string, error) {
	if len(data)%2 != 0 {
		return "", fmt.Errorf("invalid UTF-16 byte array length")
	}

	convertedToUtf16 := make([]uint16, len(data)/2)

	for i := 0; i < len(convertedToUtf16); i++ {
		convertedToUtf16[i] = binary.LittleEndian.Uint16(data[i*2:])
	}
	return string(utf16.Decode(convertedToUtf16)), nil
}
