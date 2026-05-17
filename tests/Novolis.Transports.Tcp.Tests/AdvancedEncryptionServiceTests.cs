using Novolis.Transports.Tcp.Cryptography;
using TUnit.Assertions;

namespace Novolis.Transports.Tcp.Tests;

public class AdvancedEncryptionServiceTests
{
    [Test]
    public async Task Encrypt_then_Decrypt_roundtrips_plaintext()
    {
        var options = new AdvancedEncryptionOptions
        {
            Key = "0123456789ABCDEF0123456789ABCDEF",
            Iv = "0123456789ABCDEF"
        };
        var factory = new AdvancedEncryptionFactory();
        var service = new AdvancedEncryptionService(factory, options);
        ReadOnlyMemory<byte> plain = "hello"u8.ToArray();

        var cipher = service.Encrypt(plain);
        await Assert.That(cipher.ToArray()).IsNotEqualTo(plain.ToArray());

        var roundTrip = service.Decrypt(cipher);
        await Assert.That(roundTrip.ToArray()).IsEqualTo(plain.ToArray());
    }
}
