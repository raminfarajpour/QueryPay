package billing

import (
	"context"
	"fmt"
	"github.com/raminfarajpour/database-proxy/config"
	"github.com/rs/zerolog/log"
	"google.golang.org/grpc"
	"time"
)

type BillingServiceProvider struct{}

func (b *BillingServiceProvider) CheckUserBalance(userID int64, keywords []string, rowCount int64) (bool, error) {

	configs, err := config.LoadConfig()
	if err != nil {
		log.Fatal().Msg("error in loading config")
		return false, err
	}
	grpcHost := configs.BillingService.Host
	grpcPort := configs.BillingService.Port
	if grpcHost == "" || grpcPort == 0 {
		log.Fatal().Msg("GRPC_HOST or GRPC_PORT variables are not set")
		return false, nil
	}

	serverAddress := fmt.Sprintf("%v:%v", grpcHost, grpcPort)

	conn, err := grpc.Dial(serverAddress, grpc.WithInsecure(), grpc.WithBlock())
	if err != nil {
		log.Fatal().Msgf("Failed to connect to gRPC server (%s): %v", serverAddress, err)
		return false, err
	}
	defer conn.Close()

	client := NewBillingServiceClient(conn)

	ctx, cancel := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancel()

	req := &CheckUserWalletBalanceRequest{
		UserId:   userID,
		Keywords: keywords,
		RowCount: rowCount,
	}

	resp, err := client.CheckUserWalletBalance(ctx, req)
	if err != nil {
		log.Fatal().Msgf("CheckUserWalletBalance call failed: %v", err)
		return false, err
	}

	return resp.IsBalanceSufficient, nil
}
