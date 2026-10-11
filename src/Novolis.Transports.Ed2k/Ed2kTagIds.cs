namespace Novolis.Transports.Ed2k;

/// <summary>
/// Well-known numeric tag ids used by the server and client messages.
/// </summary>
public static class Ed2kTagIds
{
    public const byte Name = 0x01;
    public const byte Size = 0x02;
    public const byte Type = 0x03;
    public const byte Format = 0x04;
    public const byte SourceCount = 0x15;
    public const byte ClientVersion = 0x11;
    public const byte ServerFlags = 0x20;
    public const byte EmuleVersion = 0xFB;
}
