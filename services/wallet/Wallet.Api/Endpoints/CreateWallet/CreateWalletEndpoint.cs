using MediatR;
using Microsoft.AspNetCore.Mvc;
using Wallet.Application.Commands.CreateWallet;

namespace Wallet.Api.Endpoints.CreateWallet;

public class CreateWalletEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/wallet",
            async ([FromBody] CreateWalletRequest request, IMediator mediator, CancellationToken cancellationToken) =>
            {
                await mediator.Send((CreateWalletCommand)request, cancellationToken);
            });
    }
}