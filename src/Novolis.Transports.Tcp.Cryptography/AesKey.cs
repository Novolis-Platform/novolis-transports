namespace Novolis.Transports.Tcp.Cryptography;

public record struct AesKey(byte[] Key, byte[] Iv);