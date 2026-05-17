using System.Net.Http.Json;
using System.Text.Json;
using Novolis.Transports.Http.Abstractions;

namespace Novolis.Transports.Http.Extensions;

public static class RestClientRestExtensions
{
    public static async Task<T?> SendAsync<T>(this IRestClient client, HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(new JsonSerializerOptions(JsonSerializerDefaults.Web), cancellationToken: cancellationToken);
    }
    
    public static async Task<HttpResponseMessage> SendAsync(this IRestClient client, HttpMethod method, string requestUri, object? content, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(method, requestUri);
        if (content != null)
        {
            request.Content = JsonContent.Create(content);
        }
        return await client.SendAsync(request, cancellationToken);
    }

    public static async Task<T?> SendAsync<T>(this IRestClient client, HttpMethod request, string requestUri, object? content, CancellationToken cancellationToken)
    {
        return await client.SendAsync<T>(new HttpRequestMessage(request, requestUri)
        {
            Content = content == null ? null : JsonContent.Create(content)
        }, cancellationToken);
    }
    
    public static async Task<T?> GetAsync<T>(this IRestClient client, string requestUri, CancellationToken cancellationToken)
    {
        return await client.SendAsync<T>(new HttpRequestMessage(HttpMethod.Get, requestUri), cancellationToken);
    }
    
    public static async Task<HttpResponseMessage> GetAsync(this IRestClient client, string requestUri, CancellationToken cancellationToken)
    {
        return await client.SendAsync(HttpMethod.Get, requestUri, null, cancellationToken);
    }
    
    public static async Task<T?> PostAsync<T>(this IRestClient client, string requestUri, object content, CancellationToken cancellationToken)
    {
        return await client.SendAsync<T>(HttpMethod.Post, requestUri, content, cancellationToken);
    }

    public static async Task<HttpResponseMessage> PostAsync(this IRestClient client, string requestUri, object content, CancellationToken cancellationToken)
    {
        return await client.SendAsync(HttpMethod.Post, requestUri, content, cancellationToken);
    }
    
    public static async Task<T?> PutAsync<T>(this IRestClient client, string requestUri, object content, CancellationToken cancellationToken)
    {
        return await client.SendAsync<T>(HttpMethod.Put, requestUri, content, cancellationToken);
    }
    
    public static async Task<HttpResponseMessage> PutAsync(this IRestClient client, string requestUri, object content, CancellationToken cancellationToken)
    {
        return await client.SendAsync(HttpMethod.Put, requestUri, content, cancellationToken);
    }
    
    public static async Task<T?> DeleteAsync<T>(this IRestClient client, string requestUri, CancellationToken cancellationToken)
    {
        return await client.SendAsync<T>(HttpMethod.Delete, requestUri, null, cancellationToken);
    }
    
    public static async Task<HttpResponseMessage> DeleteAsync(this IRestClient client, string requestUri, CancellationToken cancellationToken)
    {
        return await client.SendAsync(HttpMethod.Delete, requestUri, null, cancellationToken);
    }
    
    public static async Task<T?> PatchAsync<T>(this IRestClient client, string requestUri, object content, CancellationToken cancellationToken)
    {
        return await client.SendAsync<T>(new HttpMethod("PATCH"), requestUri, content, cancellationToken);
    }
    
    public static async Task<HttpResponseMessage> PatchAsync(this IRestClient client, string requestUri, object content, CancellationToken cancellationToken)
    {
        return await client.SendAsync(new HttpMethod("PATCH"), requestUri, content, cancellationToken);
    }
    
    public static async Task<T?> HeadAsync<T>(this IRestClient client, string requestUri, CancellationToken cancellationToken)
    {
        return await client.SendAsync<T>(HttpMethod.Head, requestUri, null, cancellationToken);
    }
    
    public static async Task<HttpResponseMessage> HeadAsync(this IRestClient client, string requestUri, CancellationToken cancellationToken)
    {
        return await client.SendAsync(HttpMethod.Head, requestUri, null, cancellationToken);
    }
    
    public static async Task<T?> OptionsAsync<T>(this IRestClient client, string requestUri, CancellationToken cancellationToken)
    {
        return await client.SendAsync<T>(HttpMethod.Options, requestUri, null, cancellationToken);
    }
    
    public static async Task<HttpResponseMessage> OptionsAsync(this IRestClient client, string requestUri, CancellationToken cancellationToken)
    {
        return await client.SendAsync(HttpMethod.Options, requestUri, null, cancellationToken);
    }
    
    public static async Task<T?> TraceAsync<T>(this IRestClient client, string requestUri, CancellationToken cancellationToken)
    {
        return await client.SendAsync<T>(HttpMethod.Trace, requestUri, null, cancellationToken);
    }
    
    public static async Task<HttpResponseMessage> TraceAsync(this IRestClient client, string requestUri, CancellationToken cancellationToken)
    {
        return await client.SendAsync(HttpMethod.Trace, requestUri, null, cancellationToken);
    }
}