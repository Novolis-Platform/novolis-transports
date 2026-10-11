namespace Novolis.Transports.Ed2k;

/// <summary>
/// A queued peer upload request.
/// </summary>
public sealed record Ed2kUploadQueueEntry(
    byte[] PeerIdentity,
    int FilePriority,
    DateTimeOffset QueuedAt);
