namespace ProtoHackersDotNet.Servers.PriceTracker;

public sealed class PriceTrackerServer(IPAddress address, ushort port)
    : TcpServer<PriceTrackerServer, PriceTrackerClient>(address, port), IServer<PriceTrackerClient>
{
    public static string ServerName => "PriceTracker";
    public static int ProblemId => 2;

    protected override PriceTrackerServer Instance => this;

    public static IServer<PriceTrackerClient> Create(IPAddress address, ushort port) => new PriceTrackerServer(address, port);

    protected override PriceTrackerClient CreateClient(TcpClient client, CancellationToken token)
        => new(client, this, token);

    public static string Description => descriptionData.Value;
    static readonly string descriptionPath = Path.Combine(ServerName, ServerName + ".md");
    static readonly Lazy<string> descriptionData = new(() => File.ReadAllText(descriptionPath));
}