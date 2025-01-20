using MediatR;

namespace Billing.Application.Commands.CreateBilling;

public record CreateBillingCommand(long UserId, string Mobile, decimal InitialBalance, decimal OverUsageThreshold)
    : IRequest<CreateBillingCommandResponse>;