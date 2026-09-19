using Microsoft.Extensions.DependencyInjection;
using Novolis.Transports.Tcp.Cryptography;

namespace Novolis.Transports.Unit.Tcp;

public sealed class TcpCryptographyDiTests
{
    [Test]
    public async Task AddTcpPayloadEncryption_RegistersEncryptor()
    {
        var services = new ServiceCollection();
        services.AddTcpPayloadEncryption(o =>
        {
            o.Key = "puDUtQJOf5UBY0iI0PwKStlBeHBEn123";
            o.Iv = "0123456789ABCDEF";
        });

        await using var provider = services.BuildServiceProvider();
        var encryptor = provider.GetRequiredService<ITcpPayloadEncryptor>();
        var payload = new byte[] { 9, 8, 7 };
        var encrypted = encryptor.Encrypt(payload);
        var decrypted = encryptor.Decrypt(encrypted);
        await Assert.That(decrypted.ToArray()).IsEquivalentTo(payload);
    }
}
