namespace Novolis.Transports.Ed2k;

/// <summary>
/// A result returned by an eD2k server search.
/// </summary>
public sealed record Ed2kSearchResult(
    Ed2kFileId FileId,
    uint ClientId,
    int Port,
    string Name,
    long Size,
    string? Type,
    int SourceCount,
    long? SpeedBytesPerSecond = null,
    int? CompleteSourceCount = null,
    int? AvailabilityPercent = null);
