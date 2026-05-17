namespace Novolis.Transports.Tcp.Cryptography;

[Obsolete("Use ITcpPayloadEncryptor. This name will be removed in a future release.")]
public interface IAdvancedEncryptionService : ITcpPayloadEncryptor;

[Obsolete("Use ITcpPayloadEncryptorFactory. This name will be removed in a future release.")]
public interface IAdvancedEncryptionFactory : ITcpPayloadEncryptorFactory;

[Obsolete("Use TcpPayloadEncryptionOptions. This name will be removed in a future release.")]
public class AdvancedEncryptionOptions : TcpPayloadEncryptionOptions;

[Obsolete("Use TcpPayloadEncryptorFactory. This name will be removed in a future release.")]
public class AdvancedEncryptionFactory : TcpPayloadEncryptorFactory, IAdvancedEncryptionFactory;

[Obsolete("Use TcpPayloadEncryptor. This name will be removed in a future release.")]
public class AdvancedEncryptionService : TcpPayloadEncryptor, IAdvancedEncryptionService
{
    public AdvancedEncryptionService(IAdvancedEncryptionFactory factory, AdvancedEncryptionOptions options)
        : base(factory, options)
    {
    }
}
