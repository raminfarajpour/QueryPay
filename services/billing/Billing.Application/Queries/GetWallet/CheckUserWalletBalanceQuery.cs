using Billing.Application.Commands.ApplyUsage;
using MediatR;

namespace Billing.Application.Queries.GetWallet;

public record CheckUserWalletBalanceQuery(long UserId,List<string> Keywords,long RowCount ):IRequest<CheckUserWalletBalanceQueryResponse>;