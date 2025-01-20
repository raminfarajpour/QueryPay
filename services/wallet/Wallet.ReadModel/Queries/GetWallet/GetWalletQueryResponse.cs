namespace Wallet.ReadModel.Queries.GetWallet;

public record GetWalletQueryResponse(Guid WalletId,long UserId,decimal Balance);