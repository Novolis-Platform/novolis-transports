using Microsoft.Extensions.DependencyInjection;

namespace Novolis.Transports.Http.Tests.Infrastructure;

internal static class TestServiceCollectionExtensions
{
    /// <summary>
    /// Routes all <see cref="IHttpClientFactory"/>-created clients through <paramref name="handler"/>.
    /// Call before <c>AddFrankHttp</c> (or any <c>AddHttpClient</c>) in tests.
    /// </summary>
    public static IServiceCollection ConfigureTestHttpHandler(this IServiceCollection services, StubHttpMessageHandler handler)
    {
        services.ConfigureHttpClientDefaults(builder =>
            builder.ConfigurePrimaryHttpMessageHandler(() => handler));
        return services;
    }
}
