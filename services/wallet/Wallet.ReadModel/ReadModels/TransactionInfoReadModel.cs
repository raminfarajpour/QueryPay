using MongoDB.Bson.Serialization.Attributes;

namespace Wallet.ReadModel.ReadModels;

public class TransactionInfoReadModel
{
    [BsonElement("transaction_id")] public string TransactionId { get; set; }
    [BsonElement("reference_id")] public string ReferenceId { get; set; }
    [BsonElement("description")] public string Description { get; set; }
}