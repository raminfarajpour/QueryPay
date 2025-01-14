using MongoDB.Bson.Serialization.Attributes;

namespace Wallet.ReadModel.ReadModels;

public class OwnerReadModel
{
    [BsonElement("userId")]
    public long UserId { get; set; }

    [BsonElement("mobile")]
    public string? Mobile { get; set; }

}