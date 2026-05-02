using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TravelAgency.Infrastructure.Options;

namespace TravelAgency.Infrastructure.Providers.Amadeus;

public sealed class AmadeusAuthClient(IHttpClientFactory factory, IMemoryCache cache, IOptions<AmadeusOptions> options, ILogger<AmadeusAuthClient> logger)
{
    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        if (cache.TryGetValue<string>("amadeus_token", out var token) && !string.IsNullOrWhiteSpace(token)) return token;

        var client = factory.CreateClient("amadeus");
        using var req = new HttpRequestMessage(HttpMethod.Post, $"{options.Value.BaseUrl}/v1/security/oauth2/token")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string> { ["grant_type"] = "client_credentials", ["client_id"] = options.Value.ClientId, ["client_secret"] = options.Value.ClientSecret })
        };

        using var res = await client.SendAsync(req, cancellationToken);
        res.EnsureSuccessStatusCode();
        using var json = JsonDocument.Parse(await res.Content.ReadAsStringAsync(cancellationToken));

        token = json.RootElement.TryGetProperty("access_token", out var tokenElem) ? tokenElem.GetString() : null;
        var expiresIn = json.RootElement.TryGetProperty("expires_in", out var expElem) ? expElem.GetInt32() : 300;

        if (string.IsNullOrWhiteSpace(token)) throw new InvalidOperationException("Amadeus auth token missing in response.");
        cache.Set("amadeus_token", token, TimeSpan.FromSeconds(Math.Max(30, expiresIn - 90)));
        logger.LogDebug("Amadeus token cache refreshed. TTL seconds: {Ttl}", Math.Max(30, expiresIn - 90));
        return token;
    }
}
