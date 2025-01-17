package events

import (
	"encoding/json"
	"fmt"
	"github.com/google/uuid"
	"time"
)

type Event struct {
	Id        string    `json:"id"`
	UserId    int64     `json:"userId"`
	Payload   string    `json:"payload"`
	CreatedAt time.Time `json:"createdAt"`
}

func NewEvent(input interface{}) (*Event, error) {
	payload, err := json.Marshal(input)
	if err != nil {
		return nil, fmt.Errorf("failed to serialize input to JSON: %w", err)
	}

	event := &Event{
		Id:        uuid.NewString(),
		UserId:    111222333, //test user id
		Payload:   string(payload),
		CreatedAt: time.Now(),
	}

	return event, nil
}
