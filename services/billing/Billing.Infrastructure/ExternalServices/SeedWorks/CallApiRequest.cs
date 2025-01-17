using System.Text;
using System.Text.Json;

namespace Billing.Infrastructure.ExternalServices.SeedWorks;

public class CallApiRequest
{
    public HttpContent? RequestContent { get; set; }
    public HttpMethod MethodType { get; set; } = HttpMethod.Get;
    public string Action { get; set; } = string.Empty;
    public IDictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();

    public static CallApiRequest Create(string action, HttpMethod methodType, object? content = null,
        JsonSerializerOptions? jsonSerializerOptions = null)
    {
        return new CallApiRequest
        {
            Action = action,
            MethodType = methodType,
            RequestContent = content != null
                ? new StringContent(content.ToJson(
                    serializerOptions: jsonSerializerOptions ?? null), Encoding.UTF8, "application/json")
                : null
        };
    }
}