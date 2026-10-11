namespace Novolis.Transports.Ed2k;

/// <summary>
/// A file advertisement sent to an eD2k server.
/// </summary>
public sealed record Ed2kSharedFile(
    Ed2kFileId FileId,
    string Name,
    long Size,
    IReadOnlyList<Ed2kTag>? AdditionalTags = null);
