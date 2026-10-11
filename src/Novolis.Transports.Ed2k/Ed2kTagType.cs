namespace Novolis.Transports.Ed2k;

/// <summary>
/// Wire value types used by eD2k and eMule tags.
/// </summary>
public enum Ed2kTagType : byte
{
    String = 0x02,
    UInt32 = 0x03,
    Binary = 0x04,
    UInt16 = 0x08,
    Byte = 0x09,
    UInt64 = 0x0B
}
