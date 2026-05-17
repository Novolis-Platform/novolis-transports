using System.Security.Cryptography;

namespace Novolis.Transports.Tcp.Cryptography;

public class AdvancedEncryptionFactory : IAdvancedEncryptionFactory
{
    public Aes Create(AesKey aesKey)
    {
        var aes = Aes.Create();
        aes.Key = aesKey.Key;
        aes.IV = aesKey.Iv;
        return aes;
    }
}
