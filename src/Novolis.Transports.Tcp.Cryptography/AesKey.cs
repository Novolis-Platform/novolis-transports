namespace Novolis.Transports.Tcp.Cryptography;

/// <summary>AES key and initialization vector pair.</summary>
/// <param name="Key">Key bytes.</param>
/// <param name="Iv">IV bytes.</param>
public record struct AesKey(byte[] Key, byte[] Iv);
