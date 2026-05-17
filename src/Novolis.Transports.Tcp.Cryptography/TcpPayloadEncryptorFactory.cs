using System.Security.Cryptography;

namespace Novolis.Transports.Tcp.Cryptography;

public class TcpPayloadEncryptorFactory : ITcpPayloadEncryptorFactory
{
    public Aes Create(AesKey aesKey)
    {
        var aes = Aes.Create();
        aes.KeySize = aesKey.Key.Length * 8;
        aes.BlockSize = 128;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = aesKey.Key;
        aes.IV = aesKey.Iv;
        return aes;
    }
}
