using Billing.Domain.Billing;
using Billing.Infrastructure;
using Billing.Infrastructure.ExternalServices.WalletService;
using Billing.Infrastructure.ExternalServices.WalletService.Models;
using Billing.Infrastructure.Persistence;
using MediatR;

namespace Billing.Application.Queries.GetWallet;

public class CheckUserWalletBalanceQueryHandler(
    IWalletProviderService walletProviderService,
    IBillingRepository billingRepository,
    BillingDbContext dbContext) : IRequestHandler<CheckUserWalletBalanceQuery, CheckUserWalletBalanceQueryResponse>
{
    public async Task<CheckUserWalletBalanceQueryResponse> Handle(CheckUserWalletBalanceQuery request,
        CancellationToken cancellationToken)
    {
        var billing = await billingRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (billing == null) throw new NullReferenceException("Billing not found");

        var commandTypes = CommandType.ConvertToCommandType(request.Keywords);

        var walletResponse =
            await walletProviderService.GetUserWalletAsync(billing.WalletId, cancellationToken);

        var financialItem = FinancialItem.Create(commandTypes, request.RowCount, Setting.PricingSetting);

        return new CheckUserWalletBalanceQueryResponse(financialItem.Amount <
                                                       new Money(walletResponse.Balance +
                                                                 walletResponse.OverUsageThreshold));
    }
}