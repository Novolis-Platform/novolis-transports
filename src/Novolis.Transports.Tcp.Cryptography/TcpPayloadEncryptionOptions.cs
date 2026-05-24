using System.Text;

namespace Novolis.Transports.Tcp.Cryptography;

/// <summary>Key material for TCP payload AES. Built-in defaults are for local development only.</summary>
public class TcpPayloadEncryptionOptions
{
    /// <summary>UTF-8 key string (length must match AES key size).</summary>
    public string Key { get; set; } = "puDUtQJOf5UBY0iI0PwKStlBeHBEn123";

    /// <summary>UTF-8 IV string (16 bytes for AES).</summary>
    public string Iv { get; set; } = "0123456789ABCDEF";

    /// <summary>Converts string key material to <see cref="AesKey"/>.</summary>
    /// <returns>Binary key and IV.</returns>
    public AesKey ToAesKey() => new(Encoding.UTF8.GetBytes(Key), Encoding.UTF8.GetBytes(Iv));
}
