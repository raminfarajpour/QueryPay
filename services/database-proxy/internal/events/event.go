package events

import (
	"encoding/json"
	"fmt"
	"github.com/google/uuid"
	"time"
)

type Event struct {
	ID        string    `json:"id"`
	Payload   string    `json:"payload"`
	CreatedAt time.Time `json:"createdAt"`
}

func NewEvent(input interface{}) (*Event, error) {
	payload, err := json.Marshal(input)
	if err != nil {
		return nil, fmt.Errorf("failed to serialize input to JSON: %w", err)
	}

	event := &Event{
		ID:        uuid.NewString(), // Generate a unique ID
		Payload:   string(payload),
		CreatedAt: time.Now(),
	}

	return event, nil
}
