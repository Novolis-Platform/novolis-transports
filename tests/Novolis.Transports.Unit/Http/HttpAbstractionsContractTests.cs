using Novolis.Transports.Http.Abstractions;
using Novolis.Transports.Http.Authentication.Basic;
using Microsoft.Extensions.Options;

namespace Novolis.Transports.Unit.Http;

public sealed class HttpAbstractionsContractTests
{
    [Test]
    public async Task BasicAuthentication_implements_IHttpAuthentication()
    {
        IHttpAuthentication auth = new BasicAuthentication(Options.Create(new BasicAuthenticationConfiguration
        {
            Username = "user",
            Password = "secret"
        }));

        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.test/resource");
        await auth.AuthenticateAsync(request, CancellationToken.None);

        await Assert.That(request.Headers.Authorization).IsNotNull();
        await Assert.That(request.Headers.Authorization!.Scheme).IsEqualTo("Basic");
    }
}
