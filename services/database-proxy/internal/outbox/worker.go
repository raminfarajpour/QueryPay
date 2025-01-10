package outbox

import (
	"context"
	"encoding/json"
	"github.com/raminfarajpour/database-proxy/internal/rabbitmq"
	"github.com/rs/zerolog/log"
	"time"
)

const (
	exchange   = "query_pay_usage_events_exchange"
	routingKey = "ue"
)

type PublishWorker struct {
	outbox     *Outbox
	publisher  *rabbitmq.Publisher
	exchange   string
	routingKey string
}

func NewPublishWorker(outbox *Outbox, publisher *rabbitmq.Publisher) *PublishWorker {
	return &PublishWorker{
		outbox:     outbox,
		publisher:  publisher,
		exchange:   exchange,
		routingKey: routingKey,
	}
}

func (w *PublishWorker) Start(ctx context.Context) {
	for {
		if ctx.Err() != nil {
			log.Fatal().Msg("publish worker stopped")
			return
		}

		events, err := w.outbox.ReadEvents(ctx)
		if err != nil {
			log.Fatal().Msgf("error in reading events: %v", err)
			time.Sleep(5 * time.Second) // Retry after a delay
			continue
		}

		for _, event := range events {
			eventData, err := json.Marshal(event)
			if err != nil {
				log.Fatal().Msgf("error in marshaling event %v: %v\n", event, err)
				continue
			}
			ctx := context.Background()
			err = w.publisher.Publish(ctx, w.exchange, w.routingKey, eventData)
			if err != nil {
				log.Fatal().Msgf("error in publishing event %v: %v\n", event, err)
				continue
			}

			if err := w.outbox.RemoveEvent(ctx, &event); err != nil {
				log.Fatal().Msgf("error in removing event %v: %v\n", event, err)
			} else {
				log.Info().Msgf("processed and removed event %v\n", event)
			}
		}

		time.Sleep(1 * time.Second)

	}
}
