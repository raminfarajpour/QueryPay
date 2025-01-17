namespace Billing.Infrastructure.ExternalServices.SeedWorks;

public abstract class HttpClientConfig
{
    public string Name { get; set; }
    public string BaseUrl { get; set; }
    public int TimeOutInSeconds { get; set; }
}