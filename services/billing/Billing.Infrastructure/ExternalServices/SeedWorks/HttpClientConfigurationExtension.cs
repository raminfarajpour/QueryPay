using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.DependencyInjection;

namespace Billing.Infrastructure.ExternalServices.SeedWorks;

public static class HttpClientConfigurationExtension
{
    public static IHttpClientBuilder AddHttpClientByConfig(this IServiceCollection services,
        HttpClientConfig httpClientConfig, HttpClientHandler? httpClientHandler = null)
    {
        var httpClientBuilder = services.AddHttpClient(httpClientConfig.Name,
            config =>
            {
                config.BaseAddress = new Uri(httpClientConfig.BaseUrl);
                if (httpClientConfig.TimeOutInSeconds != 0)
                    config.Timeout = TimeSpan.FromSeconds(httpClientConfig.TimeOutInSeconds);
            });


        httpClientBuilder.ConfigurePrimaryHttpMessageHandler(() =>
            httpClientHandler ?? new HttpClientHandler()
            {
                ClientCertificateOptions = ClientCertificateOption.Manual,
                ServerCertificateCustomValidationCallback =
                    ((Func<HttpRequestMessage, X509Certificate2, X509Chain, SslPolicyErrors, bool>)((
                        httpRequestMessage, cert, cetChain, policyErrors) => true))!
            }
        );


        return httpClientBuilder;
    }
}