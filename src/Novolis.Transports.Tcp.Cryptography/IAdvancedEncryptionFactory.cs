using System.Security.Cryptography;

namespace Novolis.Transports.Tcp.Cryptography;

public interface IAdvancedEncryptionFactory
{
    Aes Create(AesKey key);
}