using MediatR;

namespace Wallet.ReadModel.Queries.GetWallet;

public record GetWalletQuery(Guid WalletId):IRequest<GetWalletQueryResponse>;