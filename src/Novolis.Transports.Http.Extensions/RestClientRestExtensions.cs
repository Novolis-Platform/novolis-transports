using System.Net.Http.Json;
using System.Text.Json;
using Novolis.Transports.Http.Abstractions;

namespace Novolis.Transports.Http.Extensions;

/// <summary>JSON and verb helpers for <see cref="IRestClient"/>.</summary>
public static class RestClientRestExtensions
{
    /// <summary>Sends a request and deserializes a successful JSON response body.</summary>
    /// <typeparam name="T">Response DTO type.</typeparam>
    /// <param name="client">REST client.</param>
    /// <param name="request">Request message.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Deserialized body, or default when empty.</returns>
    public static async Task<T?> SendAsync<T>(this IRestClient client, HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(new JsonSerializerOptions(JsonSerializerDefaults.Web), cancellationToken: cancellationToken);
    }

    /// <summary>Sends a request with optional JSON body.</summary>
    /// <param name="client">REST client.</param>
    /// <param name="method">HTTP method.</param>
    /// <param name="requestUri">Request URI.</param>
    /// <param name="content">Optional JSON-serializable body.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The HTTP response message.</returns>
    public static async Task<HttpResponseMessage> SendAsync(this IRestClient client, HttpMethod method, string requestUri, object? content, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(method, requestUri);
        if (content != null)
        {
            request.Content = JsonContent.Create(content);
        }
        return await client.SendAsync(request, cancellationToken);
    }

    /// <summary>Sends a request with optional JSON body and deserializes the response.</summary>
    /// <typeparam name="T">Response DTO type.</typeparam>
    /// <param name="client">REST client.</param>
    /// <param name="request">HTTP method.</param>
    /// <param name="requestUri">Request URI.</param>
    /// <param name="content">Optional JSON-serializable body.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Deserialized body, or default when empty.</returns>
    public static async Task<T?> SendAsync<T>(this IRestClient client, HttpMethod request, string requestUri, object? content, CancellationToken cancellationToken)
    {
        return await client.SendAsync<T>(new HttpRequestMessage(request, requestUri)
        {
            Content = content == null ? null : JsonContent.Create(content)
        }, cancellationToken);
    }

    /// <summary>Sends GET and deserializes the JSON response.</summary>
    /// <typeparam name="T">Response DTO type.</typeparam>
    /// <param name="client">REST client.</param>
    /// <param name="requestUri">Request URI.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Deserialized body, or default when empty.</returns>
    public static async Task<T?> GetAsync<T>(this IRestClient client, string requestUri, CancellationToken cancellationToken)
    {
        return await client.SendAsync<T>(new HttpRequestMessage(HttpMethod.Get, requestUri), cancellationToken);
    }

    /// <summary>Sends GET.</summary>
    /// <param name="client">REST client.</param>
    /// <param name="requestUri">Request URI.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The HTTP response message.</returns>
    public static async Task<HttpResponseMessage> GetAsync(this IRestClient client, string requestUri, CancellationToken cancellationToken)
    {
        return await client.SendAsync(HttpMethod.Get, requestUri, null, cancellationToken);
    }

    /// <summary>Sends POST with JSON body and deserializes the response.</summary>
    /// <typeparam name="T">Response DTO type.</typeparam>
    /// <param name="client">REST client.</param>
    /// <param name="requestUri">Request URI.</param>
    /// <param name="content">JSON-serializable body.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Deserialized body, or default when empty.</returns>
    public static async Task<T?> PostAsync<T>(this IRestClient client, string requestUri, object content, CancellationToken cancellationToken)
    {
        return await client.SendAsync<T>(HttpMethod.Post, requestUri, content, cancellationToken);
    }

    /// <summary>Sends POST with JSON body.</summary>
    /// <param name="client">REST client.</param>
    /// <param name="requestUri">Request URI.</param>
    /// <param name="content">JSON-serializable body.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The HTTP response message.</returns>
    public static async Task<HttpResponseMessage> PostAsync(this IRestClient client, string requestUri, object content, CancellationToken cancellationToken)
    {
        return await client.SendAsync(HttpMethod.Post, requestUri, content, cancellationToken);
    }

    /// <summary>Sends PUT with JSON body and deserializes the response.</summary>
    /// <typeparam name="T">Response DTO type.</typeparam>
    /// <param name="client">REST client.</param>
    /// <param name="requestUri">Request URI.</param>
    /// <param name="content">JSON-serializable body.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Deserialized body, or default when empty.</returns>
    public static async Task<T?> PutAsync<T>(this IRestClient client, string requestUri, object content, CancellationToken cancellationToken)
    {
        return await client.SendAsync<T>(HttpMethod.Put, requestUri, content, cancellationToken);
    }

    /// <summary>Sends PUT with JSON body.</summary>
    /// <param name="client">REST client.</param>
    /// <param name="requestUri">Request URI.</param>
    /// <param name="content">JSON-serializable body.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The HTTP response message.</returns>
    public static async Task<HttpResponseMessage> PutAsync(this IRestClient client, string requestUri, object content, CancellationToken cancellationToken)
    {
        return await client.SendAsync(HttpMethod.Put, requestUri, content, cancellationToken);
    }

    /// <summary>Sends DELETE and deserializes the JSON response.</summary>
    /// <typeparam name="T">Response DTO type.</typeparam>
    /// <param name="client">REST client.</param>
    /// <param name="requestUri">Request URI.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Deserialized body, or default when empty.</returns>
    public static async Task<T?> DeleteAsync<T>(this IRestClient client, string requestUri, CancellationToken cancellationToken)
    {
        return await client.SendAsync<T>(HttpMethod.Delete, requestUri, null, cancellationToken);
    }

    /// <summary>Sends DELETE.</summary>
    /// <param name="client">REST client.</param>
    /// <param name="requestUri">Request URI.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The HTTP response message.</returns>
    public static async Task<HttpResponseMessage> DeleteAsync(this IRestClient client, string requestUri, CancellationToken cancellationToken)
    {
        return await client.SendAsync(HttpMethod.Delete, requestUri, null, cancellationToken);
    }

    /// <summary>Sends PATCH with JSON body and deserializes the response.</summary>
    /// <typeparam name="T">Response DTO type.</typeparam>
    /// <param name="client">REST client.</param>
    /// <param name="requestUri">Request URI.</param>
    /// <param name="content">JSON-serializable body.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Deserialized body, or default when empty.</returns>
    public static async Task<T?> PatchAsync<T>(this IRestClient client, string requestUri, object content, CancellationToken cancellationToken)
    {
        return await client.SendAsync<T>(new HttpMethod("PATCH"), requestUri, content, cancellationToken);
    }

    /// <summary>Sends PATCH with JSON body.</summary>
    /// <param name="client">REST client.</param>
    /// <param name="requestUri">Request URI.</param>
    /// <param name="content">JSON-serializable body.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The HTTP response message.</returns>
    public static async Task<HttpResponseMessage> PatchAsync(this IRestClient client, string requestUri, object content, CancellationToken cancellationToken)
    {
        return await client.SendAsync(new HttpMethod("PATCH"), requestUri, content, cancellationToken);
    }

    /// <summary>Sends HEAD and deserializes the JSON response (when present).</summary>
    /// <typeparam name="T">Response DTO type.</typeparam>
    /// <param name="client">REST client.</param>
    /// <param name="requestUri">Request URI.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Deserialized body, or default when empty.</returns>
    public static async Task<T?> HeadAsync<T>(this IRestClient client, string requestUri, CancellationToken cancellationToken)
    {
        return await client.SendAsync<T>(HttpMethod.Head, requestUri, null, cancellationToken);
    }

    /// <summary>Sends HEAD.</summary>
    /// <param name="client">REST client.</param>
    /// <param name="requestUri">Request URI.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The HTTP response message.</returns>
    public static async Task<HttpResponseMessage> HeadAsync(this IRestClient client, string requestUri, CancellationToken cancellationToken)
    {
        return await client.SendAsync(HttpMethod.Head, requestUri, null, cancellationToken);
    }

    /// <summary>Sends OPTIONS and deserializes the JSON response.</summary>
    /// <typeparam name="T">Response DTO type.</typeparam>
    /// <param name="client">REST client.</param>
    /// <param name="requestUri">Request URI.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Deserialized body, or default when empty.</returns>
    public static async Task<T?> OptionsAsync<T>(this IRestClient client, string requestUri, CancellationToken cancellationToken)
    {
        return await client.SendAsync<T>(HttpMethod.Options, requestUri, null, cancellationToken);
    }

    /// <summary>Sends OPTIONS.</summary>
    /// <param name="client">REST client.</param>
    /// <param name="requestUri">Request URI.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The HTTP response message.</returns>
    public static async Task<HttpResponseMessage> OptionsAsync(this IRestClient client, string requestUri, CancellationToken cancellationToken)
    {
        return await client.SendAsync(HttpMethod.Options, requestUri, null, cancellationToken);
    }

    /// <summary>Sends TRACE and deserializes the JSON response.</summary>
    /// <typeparam name="T">Response DTO type.</typeparam>
    /// <param name="client">REST client.</param>
    /// <param name="requestUri">Request URI.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Deserialized body, or default when empty.</returns>
    public static async Task<T?> TraceAsync<T>(this IRestClient client, string requestUri, CancellationToken cancellationToken)
    {
        return await client.SendAsync<T>(HttpMethod.Trace, requestUri, null, cancellationToken);
    }

    /// <summary>Sends TRACE.</summary>
    /// <param name="client">REST client.</param>
    /// <param name="requestUri">Request URI.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The HTTP response message.</returns>
    public static async Task<HttpResponseMessage> TraceAsync(this IRestClient client, string requestUri, CancellationToken cancellationToken)
    {
        return await client.SendAsync(HttpMethod.Trace, requestUri, null, cancellationToken);
    }
}
