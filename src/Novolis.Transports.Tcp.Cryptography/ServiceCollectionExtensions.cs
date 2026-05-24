using Microsoft.Extensions.DependencyInjection;

namespace Novolis.Transports.Tcp.Cryptography;

/// <summary>DI registration for TCP payload encryption.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Registers AES TCP payload encryptor services.</summary>
    /// <param name="services">Service collection.</param>
    /// <param name="configureOptions">Optional options callback.</param>
    /// <returns><paramref name="services"/> for chaining.</returns>
    public static IServiceCollection AddTcpPayloadEncryption(this IServiceCollection services, Action<TcpPayloadEncryptionOptions>? configureOptions = null)
    {
        var options = new TcpPayloadEncryptionOptions();
        configureOptions?.Invoke(options);

        services.AddSingleton(options);
        services.AddSingleton<ITcpPayloadEncryptorFactory, TcpPayloadEncryptorFactory>();
        services.AddSingleton<ITcpPayloadEncryptor, TcpPayloadEncryptor>();
        return services;
    }

    /// <inheritdoc cref="AddTcpPayloadEncryption(IServiceCollection, Action{TcpPayloadEncryptionOptions}?)"/>
    [Obsolete("Use AddTcpPayloadEncryption. This name will be removed in a future release.")]
    public static IServiceCollection AddAdvancedEncryption(this IServiceCollection services, Action<AdvancedEncryptionOptions>? configureOptions = null)
    {
        if (configureOptions is null)
            return services.AddTcpPayloadEncryption();

        return services.AddTcpPayloadEncryption(o =>
        {
            var legacy = new AdvancedEncryptionOptions();
            configureOptions(legacy);
            o.Key = legacy.Key;
            o.Iv = legacy.Iv;
        });
    }
}
