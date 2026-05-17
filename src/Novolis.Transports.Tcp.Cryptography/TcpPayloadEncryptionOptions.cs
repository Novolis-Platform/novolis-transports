using System.Text;

namespace Novolis.Transports.Tcp.Cryptography;

/// <summary>Key material for TCP payload AES. Built-in defaults are for local development only.</summary>
public class TcpPayloadEncryptionOptions
{
    public string Key { get; set; } = "puDUtQJOf5UBY0iI0PwKStlBeHBEn123";
    public string Iv { get; set; } = "0123456789ABCDEF";

    public AesKey ToAesKey() => new(Encoding.UTF8.GetBytes(Key), Encoding.UTF8.GetBytes(Iv));
}
