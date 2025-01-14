using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;
using Wallet.Application.IntegrationEvents;

namespace Wallet.ReadModel.ReadModels;

public class WalletReadModel
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    [BsonElement("balance")] public decimal Balance { get; set; }

    [BsonElement("overUsedThreshold")] public decimal OverUsedThreshold { get; set; }

    [BsonElement("owner")] public OwnerReadModel Owner { get; set; }

    [BsonElement("createdAt")] public DateTimeOffset CreatedAt { get; set; }

    [BsonElement("transactions")] public List<TransactionReadModel> Transactions { get; set; } = [];

    public static explicit operator WalletReadModel(WalletCreatedIntegrationEvent @event)
    {
        var wallet = new WalletReadModel
        {
            Id = @event.WalletId,
            Owner = new OwnerReadModel() { UserId = @event.Owner.UserId, Mobile = @event.Owner.Mobile },
            Balance = @event.Balance.Amount,
            OverUsedThreshold = @event.OverUsedThreshold.Amount,
            CreatedAt = @event.CreatedAt
        };
        return wallet;
    }
}