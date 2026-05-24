namespace Novolis.Transports.Tcp.Cryptography;

/// <inheritdoc cref="ITcpPayloadEncryptor"/>
[Obsolete("Use ITcpPayloadEncryptor. This name will be removed in a future release.")]
public interface IAdvancedEncryptionService : ITcpPayloadEncryptor;

/// <inheritdoc cref="ITcpPayloadEncryptorFactory"/>
[Obsolete("Use ITcpPayloadEncryptorFactory. This name will be removed in a future release.")]
public interface IAdvancedEncryptionFactory : ITcpPayloadEncryptorFactory;

/// <inheritdoc cref="TcpPayloadEncryptionOptions"/>
[Obsolete("Use TcpPayloadEncryptionOptions. This name will be removed in a future release.")]
public class AdvancedEncryptionOptions : TcpPayloadEncryptionOptions;

/// <inheritdoc cref="TcpPayloadEncryptorFactory"/>
[Obsolete("Use TcpPayloadEncryptorFactory. This name will be removed in a future release.")]
public class AdvancedEncryptionFactory : TcpPayloadEncryptorFactory, IAdvancedEncryptionFactory;

/// <inheritdoc cref="TcpPayloadEncryptor"/>
[Obsolete("Use TcpPayloadEncryptor. This name will be removed in a future release.")]
public class AdvancedEncryptionService : TcpPayloadEncryptor, IAdvancedEncryptionService
{
    /// <summary>Creates a legacy-named encryptor.</summary>
    /// <param name="factory">AES factory.</param>
    /// <param name="options">Encryption options.</param>
    public AdvancedEncryptionService(IAdvancedEncryptionFactory factory, AdvancedEncryptionOptions options)
        : base(factory, options)
    {
    }
}
