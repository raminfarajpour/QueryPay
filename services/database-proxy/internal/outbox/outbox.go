package outbox

import (
	"context"
	"encoding/json"
	"fmt"
	"github.com/go-redis/redis/v8"
	"github.com/raminfarajpour/database-proxy/internal/events"
)

type Outbox struct {
	redisClient *redis.Client
	key         string
}

const outboxKey = "outbox_events"

func NewOutbox(redisClient *redis.Client) *Outbox {
	return &Outbox{
		redisClient: redisClient,
		key:         outboxKey,
	}
}

func (outbox *Outbox) WriteEvent(context context.Context, event *events.Event) error {

	eventData, err := json.Marshal(*event)
	if err != nil {
		return fmt.Errorf("error in serializing event: %w", err)
	}

	pushErr := outbox.redisClient.RPush(context, outbox.key, eventData).Err()
	if pushErr != nil {
		return fmt.Errorf("error in writing in outbox %v", err)
	}
	return nil
}

func (outbox *Outbox) ReadEvents(ctx context.Context) ([]events.Event, error) {
	rawEvents, err := outbox.redisClient.LRange(ctx, outbox.key, 0, -1).Result()
	if err != nil {
		return nil, fmt.Errorf("error in reading events from outbox: %w", err)
	}

	var eventList []events.Event
	for _, rawEvent := range rawEvents {
		var event events.Event
		err := json.Unmarshal([]byte(rawEvent), &event)
		if err != nil {
			return nil, fmt.Errorf("error in deserializing event: %w", err)
		}
		eventList = append(eventList, event)
	}

	return eventList, nil
}

func (outbox *Outbox) RemoveEvent(ctx context.Context, event *events.Event) error {
	eventData, err := json.Marshal(*event)
	if err != nil {
		return fmt.Errorf("error in serializing event: %w", err)
	}
	err = outbox.redisClient.LRem(ctx, outbox.key, 1, eventData).Err()
	if err != nil {
		return fmt.Errorf("failed to remove event from outbox: %w", err)
	}
	return nil
}
