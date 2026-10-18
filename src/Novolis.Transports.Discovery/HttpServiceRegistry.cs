using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Novolis.Transports.Discovery;

/// <summary>
/// HTTP(S) client for a cohort service registry.
/// </summary>
/// <remarks>
/// The registry contract uses <c>PUT /v1/registrations/{cohort}/{service}/{instance}</c>
/// for registration and renewal, <c>DELETE</c> at the same path for removal,
/// and <c>GET /v1/services?cohort=...&amp;serviceName=...</c> for discovery.
/// </remarks>
public sealed class HttpServiceRegistry : IServiceRegistry
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true,
        };

    private readonly HttpClient _httpClient;
    private readonly Uri _baseAddress;

    /// <summary>Creates an HTTP(S) registry client.</summary>
    public HttpServiceRegistry(
        HttpClient httpClient,
        Uri baseAddress)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(baseAddress);
        if (!baseAddress.IsAbsoluteUri
            || baseAddress.Scheme is not ("http" or "https"))
        {
            throw new ArgumentException(
                "The registry address must be an absolute HTTP or HTTPS URI.",
                nameof(baseAddress));
        }

        _httpClient = httpClient;
        _baseAddress = new Uri(
            baseAddress.AbsoluteUri.EndsWith(
                "/",
                StringComparison.Ordinal)
                ? baseAddress.AbsoluteUri
                : baseAddress.AbsoluteUri + "/",
            UriKind.Absolute);
    }

    /// <inheritdoc />
    public async Task RegisterAsync(
        ServiceRegistration registration,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken = default)
    {
        await PutRegistrationAsync(
                registration,
                leaseDuration,
                cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task RenewAsync(
        ServiceRegistration registration,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken = default)
    {
        await PutRegistrationAsync(
                registration,
                leaseDuration,
                cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task UnregisterAsync(
        ServiceInstanceKey key,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var response = await _httpClient.DeleteAsync(
                BuildRegistrationUri(key),
                cancellationToken)
            .ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ServiceRegistration>> DiscoverAsync(
        ServiceDiscoveryQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        cancellationToken.ThrowIfCancellationRequested();

        var queryString = new StringBuilder("?cohort=")
            .Append(Uri.EscapeDataString(query.Cohort));
        if (query.ServiceName is not null)
        {
            queryString
                .Append("&serviceName=")
                .Append(Uri.EscapeDataString(query.ServiceName));
        }

        using var response = await _httpClient.GetAsync(
                new Uri(
                    _baseAddress,
                    "v1/services" + queryString),
                cancellationToken)
            .ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        await using var content = await response.Content.ReadAsStreamAsync(
                cancellationToken)
            .ConfigureAwait(false);
        using var document = await JsonDocument.ParseAsync(
                content,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        var root = document.RootElement;
        if (root.ValueKind == JsonValueKind.Object
            && root.TryGetProperty("services", out var services))
        {
            root = services;
        }

        if (root.ValueKind != JsonValueKind.Array)
            throw new JsonException(
                "The service registry response must be an array.");

        var registrations = new List<ServiceRegistration>();
        foreach (var item in root.EnumerateArray())
            registrations.Add(ReadRegistration(item));
        return registrations;
    }

    private async Task PutRegistrationAsync(
        ServiceRegistration registration,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(registration);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(
            leaseDuration,
            TimeSpan.Zero);
        cancellationToken.ThrowIfCancellationRequested();

        using var content = new ByteArrayContent(
            JsonSerializer.SerializeToUtf8Bytes(
                new
                {
                    cohort = registration.Cohort,
                    serviceName = registration.ServiceName,
                    instanceId = registration.InstanceId,
                    endpoints = registration.Endpoints.Select(endpoint => new
                    {
                        address = endpoint.Address.ToString(),
                        priority = endpoint.Priority,
                        weight = endpoint.Weight,
                    }),
                    metadata = registration.Metadata,
                    leaseDurationSeconds = leaseDuration.TotalSeconds,
                },
                JsonOptions));
        content.Headers.ContentType = new MediaTypeHeaderValue(
            "application/json");

        using var response = await _httpClient.PutAsync(
                BuildRegistrationUri(registration.Key),
                content,
                cancellationToken)
            .ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    private Uri BuildRegistrationUri(ServiceInstanceKey key) =>
        new(
            _baseAddress,
            "v1/registrations/"
            + Uri.EscapeDataString(key.Cohort)
            + "/"
            + Uri.EscapeDataString(key.ServiceName)
            + "/"
            + Uri.EscapeDataString(key.InstanceId));

    private static ServiceRegistration ReadRegistration(JsonElement item)
    {
        if (item.ValueKind != JsonValueKind.Object)
            throw new JsonException(
                "A service registry entry must be an object.");

        var endpoints = new List<ServiceEndpoint>();
        if (!item.TryGetProperty("endpoints", out var endpointArray)
            || endpointArray.ValueKind != JsonValueKind.Array)
        {
            throw new JsonException(
                "A service registry entry must contain endpoints.");
        }

        foreach (var endpoint in endpointArray.EnumerateArray())
        {
            var address = new Uri(
                endpoint.GetProperty("address").GetString()
                ?? throw new JsonException(
                    "A service endpoint address is required."),
                UriKind.Absolute);
            var priority = endpoint.TryGetProperty("priority", out var priorityValue)
                ? priorityValue.GetUInt16()
                : (ushort)0;
            var weight = endpoint.TryGetProperty("weight", out var weightValue)
                ? weightValue.GetUInt16()
                : (ushort)0;
            endpoints.Add(new ServiceEndpoint(address, priority, weight));
        }

        var metadata = new Dictionary<string, string>(
            StringComparer.OrdinalIgnoreCase);
        if (item.TryGetProperty("metadata", out var metadataObject)
            && metadataObject.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in metadataObject.EnumerateObject())
            {
                if (property.Value.ValueKind == JsonValueKind.String
                    && property.Value.GetString() is { } value)
                {
                    metadata[property.Name] = value;
                }
            }
        }

        return new ServiceRegistration(
            item.GetProperty("cohort").GetString()
                ?? throw new JsonException("A cohort is required."),
            item.GetProperty("serviceName").GetString()
                ?? throw new JsonException("A service name is required."),
            item.GetProperty("instanceId").GetString()
                ?? throw new JsonException("An instance id is required."),
            endpoints,
            metadata);
    }
}
