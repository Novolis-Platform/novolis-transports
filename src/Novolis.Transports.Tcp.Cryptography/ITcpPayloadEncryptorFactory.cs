using System.Security.Cryptography;

namespace Novolis.Transports.Tcp.Cryptography;

public interface ITcpPayloadEncryptorFactory
{
    Aes Create(AesKey key);
}
