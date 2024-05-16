using System.IO;
using ProtoHackersDotNet.Servers.Interface.Server;
using ProtoHackersDotNet.Servers.TcpServer;

namespace ProtoHackersDotNet.Servers.Echo;

public sealed class EchoServer(IPAddress address, ushort port)
    : TcpServer<EchoServer, EchoClient>(address, port), IServer<EchoClient>
{
    public static string ServerName => "Echo";
    public static int ProblemId => 0;
    protected override EchoServer Instance => this;

    public static IServer<EchoClient> Create(IPAddress ip, ushort port) => new EchoServer(ip, port);

    protected override EchoClient CreateClient(TcpClient client, CancellationToken token)
        => new(client, this, token);

    public static string Description => descriptionData.Value;
    static readonly string descriptionPath = Path.Combine(ServerName, ServerName + ".md");
    static readonly Lazy<string> descriptionData = new(() => File.ReadAllText(descriptionPath));
}
