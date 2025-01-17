using System.Text.Json;
using Microsoft.Extensions.Logging;
using Polly;

namespace Billing.Infrastructure.ExternalServices.SeedWorks;

public static class HttpClientExtensions
{
    public static async Task<CallApiResponse<TResponseEntity>> SendAsync<TResponseEntity>(
        this System.Net.Http.HttpClient client,
        CallApiRequest request,
        ILogger? logger = null,
        IAsyncPolicy<HttpResponseMessage>? policy = null,
        CancellationToken cancellationToken = default) where TResponseEntity : class, new()
    {
        var response = new CallApiResponse<TResponseEntity>();
        using var httpRequestMessage = CreateHttpRequest(request, client.BaseAddress);

        try
        {
            LogCallStart(logger!, httpRequestMessage);

            var httpResponse = policy != null
                ? await policy.ExecuteAsync(() => client.SendAsync(httpRequestMessage, cancellationToken))
                : await client.SendAsync(httpRequestMessage, cancellationToken);

            LogCallEnd(logger!, httpRequestMessage, httpResponse);

            await PopulateResponseAsync(httpResponse, httpRequestMessage, response, cancellationToken);

            if (response.RequestSucceed)
            {
                try
                {
                    response.Data =
                        System.Text.Json.JsonSerializer.Deserialize<TResponseEntity>(response.HttpResponseMessage!,
                            new JsonSerializerOptions()
                            {
                                PropertyNameCaseInsensitive = true
                            });
                    response.DeserializationSucceed = true;
                }
                catch (Exception ex)
                {
                    response.DeserializationSucceed = false;
                    response.Exception = ex;
                }
            }
        }
        catch (Exception ex)
        {
            LogException(logger!, httpRequestMessage, ex);

            HandleException(response, ex);
        }

        return response;
    }

    public static async Task<CallApiResponse> SendAsync(
        this System.Net.Http.HttpClient client,
        CallApiRequest request,
        ILogger? logger = null,
        IAsyncPolicy<HttpResponseMessage>? policy = null,
        CancellationToken cancellationToken = default)
    {
        var response = new CallApiResponse();
        using var httpRequestMessage = CreateHttpRequest(request, client.BaseAddress);

        try
        {
            LogCallStart(logger!, httpRequestMessage);

            var httpResponse = policy != null
                ? await policy.ExecuteAsync(() => client.SendAsync(CreateHttpRequest(request, client.BaseAddress), cancellationToken))
                : await client.SendAsync(httpRequestMessage, cancellationToken);

            LogCallEnd(logger!, httpRequestMessage, httpResponse);

            await PopulateResponseAsync(httpResponse, httpRequestMessage, response, cancellationToken);
        }
        catch (Exception ex)
        {
            LogException(logger!, httpRequestMessage, ex);

            HandleException(response, ex);
        }

        return response;
    }


    private static HttpRequestMessage CreateHttpRequest(CallApiRequest request, Uri? baseAddress)
    {
        var httpRequestMessage = new HttpRequestMessage
        {
            Method = request.MethodType,
            RequestUri = baseAddress != null ? new Uri(baseAddress, request.Action) : new Uri(request.Action),
            Content = request.RequestContent
        };

        foreach (var header in request.Headers)
        {
            httpRequestMessage.Headers.Add(header.Key, header.Value);
        }

        return httpRequestMessage;
    }

    private static async Task PopulateResponseAsync(
        HttpResponseMessage httpResponse,
        HttpRequestMessage httpRequestMessage,
        CallApiResponse response,
        CancellationToken cancellationToken)
    {
        response.HttpResponseMessage = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
        response.ResponseHeader = httpResponse.Headers.ToDictionary(h => h.Key, h => h.Value.FirstOrDefault());
        response.StatusCode = httpResponse.StatusCode;
        response.RequestSucceed = httpResponse.IsSuccessStatusCode;
        response.RequestUri = httpRequestMessage.RequestUri?.ToString();

        try
        {
            response.RequestContent = httpRequestMessage.Content != null
                ? await httpRequestMessage.Content.ReadAsStringAsync(cancellationToken)
                : "Request Content Is Empty";
        }
        catch
        {
            // Ignore errors when retrieving request content
        }
    }

    private static void LogCallStart(ILogger? logger, HttpRequestMessage requestMessage)
    {
        logger?.LogInformation(
            $"Calling {requestMessage.RequestUri} Started " +
            $"With Content: {requestMessage?.Content?.ToJson()}");
    }

    private static void LogCallEnd(ILogger? logger, HttpRequestMessage requestMessage,
        HttpResponseMessage? httpResponseMessage)
    {
        logger?.LogInformation(
            $"Calling {requestMessage.RequestUri} Ended " +
            $"With Content: {requestMessage?.Content?.ToJson()}" +
            $"With Response: {httpResponseMessage?.ToJson()}");
    }

    private static void LogException(ILogger? logger, HttpRequestMessage requestMessage, Exception exception)
    {
        logger?.LogError(exception,
            $"Calling {requestMessage.RequestUri} Throws Exception " +
            $"With Content: {requestMessage?.Content?.ToJson()}");
    }

    private static void HandleException(CallApiResponse response, Exception ex)
    {
        response.ErrorMessage = $"ServiceCall encountered an exception: {ex.Message}";
        response.Exception = ex;
    }
}