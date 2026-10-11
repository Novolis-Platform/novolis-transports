using System.Net;

namespace Novolis.Transports.NetworkPath;

/// <summary>One adapter-bound connection attempt.</summary>
public sealed record NetworkProbeSample(
    TimeSpan? Duration,
    bool Succeeded,
    IPAddress? SourceAddress,
    string? Error);
