using MediatR;
using Wallet.ReadModel.Repositories;

namespace Wallet.ReadModel.Queries.GetWallet;

public class GetWalletQueryHandler(IWalletReadModelRepository walletReadModelRepository) : IRequestHandler<GetWalletQuery, GetWalletQueryResponse>
{
    public async Task<GetWalletQueryResponse> Handle(GetWalletQuery request, CancellationToken cancellationToken)
    {
        var wallet= await walletReadModelRepository.GetByIdAsync(request.WalletId,cancellationToken);
        return new GetWalletQueryResponse(wallet.Id,wallet.Owner.UserId,wallet.Balance);
    }
}