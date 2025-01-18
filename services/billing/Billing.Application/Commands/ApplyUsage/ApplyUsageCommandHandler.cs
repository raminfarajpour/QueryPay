using Billing.Domain.Billing;
using Billing.Infrastructure;
using Billing.Infrastructure.ExternalServices.WalletService;
using Billing.Infrastructure.ExternalServices.WalletService.Models;
using Billing.Infrastructure.Persistence;
using MediatR;

namespace Billing.Application.Commands.ApplyUsage;

public class ApplyUsageCommandHandler(
    IWalletProviderService walletProviderService,
    IBillingRepository billingRepository,
    BillingDbContext dbContext) : IRequestHandler<ApplyUsageCommand>
{

    public async Task Handle(ApplyUsageCommand request, CancellationToken cancellationToken)
    {
        var billing = await billingRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (billing == null) throw new NullReferenceException("Billing not found");

        var commandTypes = CommandType.ConvertToCommandType(request.Keywords);

        var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        
        var financialItem = FinancialItem.Create(commandTypes, request.RowCount,Setting.PricingSetting);
        billing.AddFinancialItem(financialItem);
        await dbContext.SaveChangesAsync(cancellationToken);

        var withdrawRequest = new WithdrawRequestModel(financialItem.Amount.Amount,financialItem.Id.ToString(),"Usage Charge");
        var withdrawResponse =
            await walletProviderService.WithdrawAsync(billing.WalletId,withdrawRequest , cancellationToken);
        
        billing.UpdateBalance(new Money(withdrawResponse.Balance));
        await dbContext.SaveChangesAsync(cancellationToken);
        
        await transaction.CommitAsync(cancellationToken);
    }


    
}