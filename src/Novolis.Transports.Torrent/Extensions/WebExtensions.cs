using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using DefensiveProgrammingFramework;

namespace Novolis.Transports.Torrent.Extensions;

/// <summary>
///     The web related extensions.
/// </summary>
public static class WebExtensions
{
    /// <summary>
    ///     Executes the binary request.
    /// </summary>
    /// <param name="uri">The URI.</param>
    /// <param name="timeout">The timeout.</param>
    /// <returns>
    ///     The binary data.
    /// </returns>
    public static byte[] ExecuteBinaryRequest(this Uri uri, TimeSpan? timeout = null)
    {
        Stopwatch stopwatch;
        byte[] responseContent;
        Func<HttpResponseMessage, byte[]> getDataFunc;

        getDataFunc = response =>
        {
            int count;
            var content = new List<byte>();
            var buffer = new byte[4096];

            using (var responseStream = response.Content.ReadAsStream())
            {
                while ((count = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                    content.AddRange(buffer.Take(count));
            }

            return content.ToArray();
        };

        stopwatch = Stopwatch.StartNew();

        responseContent = uri.ExecuteRequest(getDataFunc, null, "text/plain", 0, timeout ?? TimeSpan.FromMinutes(1));

        stopwatch.Stop();

        return responseContent;
    }

    /// <summary>
    ///     Executes the UDP request.
    /// </summary>
    /// <param name="endpoint">The endpoint.</param>
    /// <param name="bytes">The payload bytes.</param>
    public static void ExecuteUdpRequest(this IPEndPoint endpoint, byte[] bytes)
    {
        endpoint.CannotBeNull();
        bytes.CannotBeNullOrEmpty();

        var bufferSize = 4096;
        var sendTimeout = TimeSpan.FromSeconds(5);
        var receiveTimeout = TimeSpan.FromSeconds(5);
        IAsyncResult asyncResult;
        int count;

        using (var udp = new UdpClient())
        {
            udp.Client.SendTimeout = (int)sendTimeout.TotalMilliseconds;
            udp.Client.SendBufferSize = bytes.Length;
            udp.Client.ReceiveTimeout = (int)receiveTimeout.TotalMilliseconds;
            udp.Client.ReceiveBufferSize = bufferSize;
            udp.Connect(endpoint);

            asyncResult = udp.BeginSend(bytes, bytes.Length, null, null);

            if (asyncResult.AsyncWaitHandle.WaitOne(receiveTimeout))
            {
                count = udp.EndSend(asyncResult);

                if (count == bytes.Length)
                {
                    // ok
                }
            }

            udp.Close();
        }
    }

    /// <summary>
    ///     Executes the HTTP request.
    /// </summary>
    /// <typeparam name="T">The return type.</typeparam>
    /// <param name="uri">The URI.</param>
    /// <param name="getDataFunc">The get data function.</param>
    /// <param name="data">The data.</param>
    /// <param name="requestContentType">Type of the request content.</param>
    /// <param name="redirectCount">The redirect count.</param>
    /// <param name="timeout">The timeout.</param>
    /// <returns>
    ///     The response result.
    /// </returns>
    private static T ExecuteRequest<T>(this Uri uri, Func<HttpResponseMessage, T> getDataFunc, byte[] data = null,
        string requestContentType = "text/plain", int redirectCount = 0, TimeSpan? timeout = null)
    {
        var responseContent = default(T);
        var maxRedirects = 30;

        uri.CannotBeNull();
        getDataFunc.CannotBeNull();
        requestContentType.CannotBeNullOrEmpty();
        redirectCount.MustBeGreaterThanOrEqualTo(0);

        using var handler = new SocketsHttpHandler
        {
            AllowAutoRedirect = false,
            AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
            UseCookies = true,
            CookieContainer = new CookieContainer(),
        };
        using var client = new HttpClient(handler)
        {
            Timeout = timeout ?? TimeSpan.FromSeconds(10),
        };

        using var request = new HttpRequestMessage(data == null ? HttpMethod.Get : HttpMethod.Post, uri);
        request.Headers.ConnectionClose = true;
        request.Headers.TryAddWithoutValidation(
            "User-Agent",
            "Mozilla/5.0 (Windows NT 5.1) AppleWebKit/537.11 (KHTML, like Gecko) Chrome/23.0.1300.0 Iron/23.0.1300.0 Safari/537.11");

        if (data != null)
        {
            request.Content = new ByteArrayContent(data);
            request.Content.Headers.TryAddWithoutValidation("Content-Type", requestContentType);
        }

        using var response = client.Send(request);
        if (response.StatusCode == HttpStatusCode.Redirect)
        {
            if (redirectCount <= maxRedirects &&
                response.Headers.Location is { IsAbsoluteUri: true } redirect)
            {
                responseContent = redirect.ExecuteRequest(getDataFunc, data, requestContentType, ++redirectCount, timeout);
            }
        }
        else
        {
            responseContent = getDataFunc(response);
        }

        return responseContent;
    }
}
