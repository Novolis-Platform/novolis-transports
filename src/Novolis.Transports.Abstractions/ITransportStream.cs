namespace Novolis.Transports;

/// <summary>Owns one reliable ordered byte stream.</summary>
public interface ITransportStream : IAsyncDisposable
{
    /// <summary>Gets the stream used for framed application payloads.</summary>
    Stream Stream { get; }
}
