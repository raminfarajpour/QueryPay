using System.Text.Json.Serialization;
using Billing.Infrastructure.SeedWorks.Integration;
using Newtonsoft.Json;

namespace Billing.Application.MessageHandlers;

public class UsageMessage:IIntegrationEvent
{
    public Guid Id { get; set; }
    public long UserId { get; set; }
    public string Payload { get; set; }
    public DateTime CreatedAt { get; set; }

    public UsageMessagePayload GetPayload()
    {
        return JsonConvert.DeserializeObject<UsageMessagePayload>(Payload)!;
    }
}