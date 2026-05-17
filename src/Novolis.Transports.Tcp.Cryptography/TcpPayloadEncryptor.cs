namespace Novolis.Transports.Tcp.Cryptography;

public class TcpPayloadEncryptor : ITcpPayloadEncryptor
{
    private readonly ITcpPayloadEncryptorFactory _factory;
    private readonly TcpPayloadEncryptionOptions _options;

    public TcpPayloadEncryptor(ITcpPayloadEncryptorFactory factory, TcpPayloadEncryptionOptions options)
    {
        _factory = factory;
        _options = options;
    }

    public ReadOnlyMemory<byte> Encrypt(ReadOnlyMemory<byte> data)
    {
        using var aes = _factory.Create(_options.ToAesKey());
        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        return encryptor.TransformFinalBlock(data.ToArray(), 0, data.Length);
    }

    public ReadOnlyMemory<byte> Decrypt(ReadOnlyMemory<byte> data)
    {
        using var aes = _factory.Create(_options.ToAesKey());
        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        return decryptor.TransformFinalBlock(data.ToArray(), 0, data.Length);
    }
}
