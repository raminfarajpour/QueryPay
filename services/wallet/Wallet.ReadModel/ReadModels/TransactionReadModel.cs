using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Wallet.Application.IntegrationEvents;
using Wallet.Domain.WalletAggregate;

namespace Wallet.ReadModel.ReadModels;

public class TransactionReadModel
{
    [BsonElement("id")]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    [BsonElement("amount")] public decimal Amount { get; set; }
    [BsonElement("balance_before")] public decimal BalanceBefore { get; set; }
    [BsonElement("balance_after")] public decimal BalanceAfter { get; set; }
    [BsonElement("direction")] public TransactionDirection Direction { get; set; }
    [BsonElement("created_at")] public DateTimeOffset CreatedAt { get; set; }
    [BsonElement("transaction_info")] public TransactionInfoReadModel TransactionInfo { get; set; }

    public static explicit operator TransactionReadModel(WalletTransactionCreatedIntegrationEvent @event)
    {
        return new TransactionReadModel()
        {
            Id = @event.Transaction.Id,
            TransactionInfo = new TransactionInfoReadModel()
            {
                Description = @event.Transaction.TransactionInfo.Description,
                ReferenceId = @event.Transaction.TransactionInfo.ReferenceId,
                TransactionId = @event.Transaction.TransactionInfo.TransactionId
            },
            CreatedAt = @event.Transaction.CreatedAt,
            BalanceBefore = @event.Transaction.BalanceBefore.Amount,
            BalanceAfter = @event.Transaction.BalanceAfter.Amount,
            Direction = @event.Transaction.Direction,
            Amount = @event.Transaction.Amount.Amount,
        };
    }
}