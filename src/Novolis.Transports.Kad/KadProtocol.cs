namespace Novolis.Transports.Kad;

/// <summary>
/// Kad protocol marker and opcode constants.
/// </summary>
public static class KadProtocol
{
    public const byte Header = 0xE4;
    public const byte Packed = 0xE5;

    public const byte BootstrapRequest = 0x01;
    public const byte BootstrapResponse = 0x09;
    public const byte HelloRequest = 0x11;
    public const byte HelloResponse = 0x19;
    public const byte NodeRequest = 0x21;
    public const byte NodeResponse = 0x29;
    public const byte SearchKeyRequest = 0x33;
    public const byte SearchSourceRequest = 0x34;
    public const byte SearchNotesRequest = 0x35;
    public const byte SearchResponse = 0x3B;
    public const byte PublishKeyRequest = 0x43;
    public const byte PublishSourceRequest = 0x44;
    public const byte PublishNotesRequest = 0x45;
    public const byte PublishKeyResponse = 0x4B;
    public const byte PublishSourceResponse = 0x4C;
    public const byte Ping = 0x60;
    public const byte Pong = 0x61;
}
