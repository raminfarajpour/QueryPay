using MediatR;
using Microsoft.AspNetCore.Mvc;
using Wallet.Application.Commands.Withdraw;

namespace Wallet.Api.Endpoints.Withdraw;

public class WithdrawEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("api/wallet/{walletId}/withdraw",
            async ([FromRoute] Guid walletId, [FromBody] WithdrawRequest request, IMediator mediator,
                CancellationToken cancellationToken) =>
            {
                var withdrawCommand = (WithdrawCommand)request with
                {
                    WalletId = walletId
                };

                var result = await mediator.Send(withdrawCommand, cancellationToken);
                return Results.Ok(result);
            });
    }
}