using Billing.Domain.Billing;
using Billing.Infrastructure.ExternalServices.WalletService;
using Billing.Infrastructure.ExternalServices.WalletService.Models;
using Billing.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Billing.Application.Commands.CreateBilling;

public class CreateBillingCommandHandler(
    IWalletProviderService walletProviderService,
    IBillingRepository billingRepository,
    BillingDbContext dbContext) : IRequestHandler<CreateBillingCommand,CreateBillingCommandResponse>
{
    public async Task<CreateBillingCommandResponse> Handle(CreateBillingCommand request, CancellationToken cancellationToken)
    {
        var walletResponse = await walletProviderService.CreateUserWallet(
            new CreateWalletRequestModel(request.UserId, request.Mobile, request.InitialBalance,
                request.OverUsageThreshold), cancellationToken);
        
        var billing = new BillingAggregate(request.UserId,walletResponse.WalletId,new Money(walletResponse.Balance));
        
        await billingRepository.CreateAsync(billing, cancellationToken);
        
        await dbContext.SaveChangesAsync(cancellationToken);

        return new CreateBillingCommandResponse(walletResponse.WalletId);
    }
}