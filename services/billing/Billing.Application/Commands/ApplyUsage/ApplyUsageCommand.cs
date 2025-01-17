using MediatR;

namespace Billing.Application.Commands.ApplyUsage;

public record ApplyUsageCommand(long UserId, List<string> Keywords,long RowCount):IRequest;


public class ApplyUsageCommandHandler : IRequestHandler<ApplyUsageCommand>
{
    public Task Handle(ApplyUsageCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}