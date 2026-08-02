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

    [Test]
    [Obsolete("Covers legacy AddAdvancedEncryption registration.")]
    public async Task AddAdvancedEncryption_LegacyAlias_UsesSameOptions()
    {
#pragma warning disable CS0618
        var services = new ServiceCollection();
        services.AddAdvancedEncryption(o =>
        {
            o.Key = "puDUtQJOf5UBY0iI0PwKStlBeHBEn123";
            o.Iv = "0123456789ABCDEF";
        });

        await using var provider = services.BuildServiceProvider();
        var encryptor = provider.GetRequiredService<ITcpPayloadEncryptor>();
        var legacy = new AdvancedEncryptionService(
            new AdvancedEncryptionFactory(),
            new AdvancedEncryptionOptions
            {
                Key = "puDUtQJOf5UBY0iI0PwKStlBeHBEn123",
                Iv = "0123456789ABCDEF",
            });
        var roundTrip = legacy.Decrypt(legacy.Encrypt(new ReadOnlyMemory<byte>(new byte[] { 1, 2, 3 })));
#pragma warning restore CS0618
        await Assert.That(roundTrip.ToArray()).IsEquivalentTo(new byte[] { 1, 2, 3 });
        await Assert.That(encryptor).IsNotNull();
    }

    [Test]
    [Obsolete("Covers legacy AdvancedEncryptionService type.")]
    public async Task AdvancedEncryptionService_LegacyTypesConstruct()
    {
#pragma warning disable CS0618
        var factory = new AdvancedEncryptionFactory();
        var options = new AdvancedEncryptionOptions
        {
            Key = "puDUtQJOf5UBY0iI0PwKStlBeHBEn123",
            Iv = "0123456789ABCDEF",
        };
        var service = new AdvancedEncryptionService(factory, options);
        await Assert.That(service.Decrypt(service.Encrypt(new ReadOnlyMemory<byte>(new byte[] { 4, 5 }))).ToArray())
            .IsEquivalentTo(new byte[] { 4, 5 });
#pragma warning restore CS0618
    }
}
