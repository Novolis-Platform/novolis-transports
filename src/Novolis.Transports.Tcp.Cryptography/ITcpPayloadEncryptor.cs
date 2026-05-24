namespace Novolis.Transports.Tcp.Cryptography;

/// <summary>Encrypts and decrypts TCP message payloads with AES.</summary>
public interface ITcpPayloadEncryptor
{
    /// <summary>Encrypts a payload.</summary>
    /// <param name="data">Plaintext bytes.</param>
    /// <returns>Ciphertext bytes.</returns>
    ReadOnlyMemory<byte> Encrypt(ReadOnlyMemory<byte> data);

    /// <summary>Decrypts a payload.</summary>
    /// <param name="data">Ciphertext bytes.</param>
    /// <returns>Plaintext bytes.</returns>
    ReadOnlyMemory<byte> Decrypt(ReadOnlyMemory<byte> data);
}
