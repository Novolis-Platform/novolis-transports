using System.Security.Cryptography;

namespace Novolis.Transports.Ed2k;

/// <summary>
/// Isolated RSA challenge-response helper for Secure User Identification.
/// </summary>
public sealed class Ed2kSecureIdentity : IDisposable
{
    private readonly RSA key;

    public Ed2kSecureIdentity(int keySize = 1024)
    {
        if (keySize < 512)
            throw new ArgumentOutOfRangeException(nameof(keySize));
        this.key = RSA.Create(keySize);
    }

    public byte[] PublicKey => this.key.ExportSubjectPublicKeyInfo();

    public byte[] CreateChallenge(int length = 16)
    {
        if (length is < 1 or > 4096)
            throw new ArgumentOutOfRangeException(nameof(length));
        var challenge = new byte[length];
        RandomNumberGenerator.Fill(challenge);
        return challenge;
    }

    public byte[] Sign(ReadOnlySpan<byte> challenge)
    {
        return this.key.SignData(
            challenge,
            HashAlgorithmName.SHA1,
            RSASignaturePadding.Pkcs1);
    }

    public bool Verify(
        ReadOnlySpan<byte> challenge,
        ReadOnlySpan<byte> signature)
    {
        return Verify(this.PublicKey, challenge, signature);
    }

    public static bool Verify(
        ReadOnlySpan<byte> publicKey,
        ReadOnlySpan<byte> challenge,
        ReadOnlySpan<byte> signature)
    {
        try
        {
            using var key = RSA.Create();
            key.ImportSubjectPublicKeyInfo(publicKey, out _);
            return key.VerifyData(
                challenge,
                signature,
                HashAlgorithmName.SHA1,
                RSASignaturePadding.Pkcs1);
        }
        catch (CryptographicException)
        {
            return false;
        }
    }

    public void Dispose()
    {
        this.key.Dispose();
        GC.SuppressFinalize(this);
    }
}
