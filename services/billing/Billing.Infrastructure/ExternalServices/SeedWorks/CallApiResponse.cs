using System.Net;

namespace Billing.Infrastructure.ExternalServices.SeedWorks;

public class CallApiResponse<TResponse> : CallApiResponse where TResponse : class, new()
{
    public TResponse? Data { get; set; }
}

public class CallApiResponse
{
    public string? HttpResponseMessage { get; set; }
    public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.RequestTimeout;
    public bool RequestSucceed { get; set; } = false;
    public bool DeserializationSucceed { get; set; } = false;
    public string? ErrorMessage { get; set; }
    public string? RequestUri { get; set; }
    public Exception? Exception { get; set; }
    public string? RequestContent { get; set; }
    public Dictionary<string, string?>? ResponseHeader { get; set; } = new();
}