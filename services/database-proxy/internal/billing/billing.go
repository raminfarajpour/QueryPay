package billing

import (
	"context"
	"fmt"
	"github.com/raminfarajpour/database-proxy/config"
	"github.com/rs/zerolog/log"
	"google.golang.org/grpc"
	"google.golang.org/grpc/credentials"
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

	creds, err := credentials.NewClientTLSFromFile(configs.Certificate.File, "")
	if err != nil {
		log.Fatal().Msgf("Failed to create TLS credentials %v", err)
	}

	conn, err := grpc.NewClient(serverAddress, grpc.WithTransportCredentials(creds))
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

	log.Info().Msgf("User Balance Response  %v", resp)
	return resp.IsBalanceSufficient, nil
}
