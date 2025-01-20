using MediatR;

namespace Billing.Application.Commands.ApplyUsage;

public record ApplyUsageCommand(long UserId, List<string> Keywords, long RowCount) : IRequest;