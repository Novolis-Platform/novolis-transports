using System.Security.Cryptography;

namespace Novolis.Transports.Tcp.Cryptography;

/// <summary>Creates configured <see cref="Aes"/> instances for TCP payload crypto.</summary>
public interface ITcpPayloadEncryptorFactory
{
    /// <summary>Creates an AES instance for <paramref name="key"/>.</summary>
    /// <param name="key">Key and IV material.</param>
    /// <returns>Configured AES instance (caller must dispose).</returns>
    Aes Create(AesKey key);
}
