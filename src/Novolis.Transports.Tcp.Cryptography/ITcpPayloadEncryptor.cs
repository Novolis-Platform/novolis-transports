namespace Novolis.Transports.Tcp.Cryptography;

public interface ITcpPayloadEncryptor
{
    ReadOnlyMemory<byte> Encrypt(ReadOnlyMemory<byte> data);

    ReadOnlyMemory<byte> Decrypt(ReadOnlyMemory<byte> data);
}
