using ProtoHackersDotNet.Servers.Interface.Server;
using ProtoHackersDotNet.Servers.TcpServer;
using System.IO;

namespace ProtoHackersDotNet.Servers.JsonPrime;

public sealed class JsonPrimeServer(IPAddress ip, ushort port)
    : TcpServer<JsonPrimeServer, JsonPrimeClient>(ip, port), IServer<JsonPrimeClient>
{
    public static string ServerName => "JsonPrime";
    public static int ProblemId => 1;
    protected override JsonPrimeServer Instance => this;

    public static IServer<JsonPrimeClient> Create(IPAddress ip, ushort port) => new JsonPrimeServer(ip, port);

    protected override JsonPrimeClient CreateClient(TcpClient client, CancellationToken token)
        => new(client, this, token);

    public static string Description => descriptionData.Value;
    static readonly string descriptionPath = Path.Combine(ServerName, ServerName + ".md");
    static readonly Lazy<string> descriptionData = new(() => File.ReadAllText(descriptionPath));
}