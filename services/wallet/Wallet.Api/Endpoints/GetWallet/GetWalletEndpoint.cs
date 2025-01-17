using MediatR;
using Microsoft.AspNetCore.Mvc;
using Wallet.ReadModel.Queries.GetWallet;

namespace Wallet.Api.Endpoints.GetWallet;

public class GetWalletEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/wallet/{walletId}",
            async ([FromRoute] Guid walletId,IMediator mediator,
                CancellationToken cancellationToken) =>
            {
                var query = new GetWalletQuery(walletId);

                var result = await mediator.Send(query, cancellationToken);
                return Results.Ok(result);
            });
    }
}