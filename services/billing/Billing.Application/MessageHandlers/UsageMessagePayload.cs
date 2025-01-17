namespace Billing.Application.MessageHandlers;

public class UsageMessagePayload{
    public List<string> Keywords { get; set; }
    public long RowCount { get; set; }
}

