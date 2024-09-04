using System.Net.Http;
using System.Net.Http.Json;

namespace ProtoHackersDotNet.GUI.MainView.IpIfyClient;

public class IpIfyClient(HttpClient client, IpIfyClientOptions options)
{
    public async Task<IPAddress> GetExternalIP() => await GetExternalIP(CancellationToken.None);

    public async Task<IPAddress> GetExternalIP(CancellationToken token = default) 
        => (await client.GetFromJsonAsync(options.IPv46Url, IpIfyMetaData.Default.IPResponse, token))?.Ip
                ?? ThrowResponseError<IPAddress>();

    public async Task<IPAddress> GetExternalIPv4(CancellationToken token = default)
        => (await client.GetFromJsonAsync(options.IPv4Url, IpIfyMetaData.Default.IPResponse, token))?.Ip 
            ?? ThrowResponseError<IPAddress>();

    public async Task<IPAddress> GetExternalIPv6(CancellationToken token = default)
        => (await client.GetFromJsonAsync(options.IPv6Url, IpIfyMetaData.Default.IPResponse, token))?.Ip 
            ?? ThrowResponseError<IPAddress>();

    public static void Configure(IServiceProvider _, HttpClient client)
        => client.DefaultRequestHeaders.UserAgent.Add(App.UserAgent);

    [DoesNotReturn] public static T ThrowResponseError<T>() => throw new HttpRequestException("Error response from api");
}
