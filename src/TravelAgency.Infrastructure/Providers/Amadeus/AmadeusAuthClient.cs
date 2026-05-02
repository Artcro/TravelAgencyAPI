using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using TravelAgency.Infrastructure.Options;

namespace TravelAgency.Infrastructure.Providers.Amadeus;

public sealed class AmadeusAuthClient(IHttpClientFactory factory, IMemoryCache cache, IOptions<AmadeusOptions> options)
{
    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        if (cache.TryGetValue<string>("amadeus_token", out var token) && !string.IsNullOrWhiteSpace(token)) return token;
        var client = factory.CreateClient();
        var req = new HttpRequestMessage(HttpMethod.Post, $"{options.Value.BaseUrl}/v1/security/oauth2/token")
        { Content = new FormUrlEncodedContent(new Dictionary<string, string> { ["grant_type"] = "client_credentials", ["client_id"] = options.Value.ClientId, ["client_secret"] = options.Value.ClientSecret }) };
        var res = await client.SendAsync(req, cancellationToken); res.EnsureSuccessStatusCode();
        var json = JsonDocument.Parse(await res.Content.ReadAsStringAsync(cancellationToken));
        token = json.RootElement.GetProperty("access_token").GetString()!;
        var expiresIn = json.RootElement.GetProperty("expires_in").GetInt32();
        cache.Set("amadeus_token", token, TimeSpan.FromSeconds(Math.Max(30, expiresIn - 60)));
        return token;
    }
}
