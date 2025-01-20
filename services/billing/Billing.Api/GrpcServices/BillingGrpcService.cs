using Billing.Api.ProtoFiles;
using Billing.Application.Commands.CreateBilling;
using Billing.Application.Queries.GetWallet;
using Grpc.Core;
using MediatR;

namespace Billing.Api.GrpcServices;

public class BillingGrpcService(IMediator mediator):BillingService.BillingServiceBase
{
     public override async Task<CheckUserWalletBalanceResponse> CheckUserWalletBalance(CheckUserWalletBalanceRequest request, ServerCallContext context)
     {
         try
         {
             var query = new CheckUserWalletBalanceQuery(
                 UserId: request.UserId,
                 Keywords: request.Keywords.ToList(),
                 RowCount: request.RowCount
             );

             var response = await mediator.Send(query, context.CancellationToken);

             return new CheckUserWalletBalanceResponse
             {
                 IsBalanceSufficient = response.CanPerformOperation
             };
         }
         catch (NullReferenceException ex)
         {
             throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
         }
         catch (Exception ex)
         {
             throw new RpcException(new Status(StatusCode.Internal, "Error in checking wallet balance."));
         }
     }
     public override async Task<CreateBillingResponse> CreateBilling(CreateBillingRequest request, ServerCallContext context)
     {
         try
         {
             var command = new CreateBillingCommand(
                 UserId: request.UserId,
                 Mobile: request.Mobile,
                 InitialBalance: (decimal)request.InitialBalance,           
                 OverUsageThreshold: (decimal)request.OverUsageThreshold    
             );
     
             var createResponse = await mediator.Send(command, context.CancellationToken);
     
             return new CreateBillingResponse
             {
                 Success = true,
                 Message = "Billing created successfully.",
                 WalletId = createResponse.WalletId.ToString()
             };
         }
         catch (NullReferenceException ex)
         {
             throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
         }
         catch (Exception ex)
         {
             throw new RpcException(new Status(StatusCode.Internal, $"error in creating billing: {ex.Message}"));
         }
     }
}