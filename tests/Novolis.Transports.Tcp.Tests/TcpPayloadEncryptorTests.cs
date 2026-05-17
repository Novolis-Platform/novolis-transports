using Novolis.Transports.Tcp.Cryptography;
using TUnit.Core;

namespace Novolis.Transports.Tcp.Tests;

public class TcpPayloadEncryptorTests
{
    [Test]
    public async Task EncryptDecrypt_RoundTripsPayload()
    {
        var options = new TcpPayloadEncryptionOptions
        {
            Key = "puDUtQJOf5UBY0iI0PwKStlBeHBEn123",
            Iv = "0123456789ABCDEF"
        };
        var factory = new TcpPayloadEncryptorFactory();
        var service = new TcpPayloadEncryptor(factory, options);
        var payload = new byte[] { 1, 2, 3, 4, 5 };

        var encrypted = service.Encrypt(payload);
        var decrypted = service.Decrypt(encrypted);

        await Assert.That(decrypted.ToArray().SequenceEqual(payload)).IsTrue();
    }
}
