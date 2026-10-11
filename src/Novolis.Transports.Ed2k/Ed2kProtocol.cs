namespace Novolis.Transports.Ed2k;

/// <summary>
/// Protocol constants shared by eD2k server, peer, and file metadata messages.
/// </summary>
public static class Ed2kProtocol
{
    public const byte EdonkeyProtocol = 0xE3;
    public const byte EmuleProtocol = 0xC5;
    public const byte ZlibProtocol = 0xD4;

    public const byte LoginRequest = 0x01;
    public const byte LoginAnswer = 0x01;
    public const byte IdChange = 0x40;
    public const byte OfferFiles = 0x15;
    public const byte SearchRequest = 0x16;
    public const byte SearchResults = 0x33;
    public const byte GetSources = 0x58;
    public const byte Sources = 0x59;
    public const byte ServerMessage = 0x38;
    public const byte ServerStatus = 0x34;

    public const byte PeerHello = 0x01;
    public const byte PeerHelloAnswer = 0x4C;
    public const byte PeerFileRequest = 0x58;
    public const byte PeerFileStatus = 0x50;
    public const byte PeerStartUpload = 0x55;
    public const byte PeerSendingPart = 0x46;
    public const byte PeerRequestParts = 0x47;
    public const byte PeerHashSetRequest = 0x51;
    public const byte PeerHashSetAnswer = 0x52;

    public const int PartSize = 9_728_000;
    public const int AichBlockSize = 184_320;
    public const int MaximumFramePayload = 16 * 1024 * 1024;
}
