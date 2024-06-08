namespace ProtoHackersDotNet.Servers.PriceTracker;

public sealed class PriceTrackerServer : TcpServerBase<PriceTrackerClient>
{
    public override ServerName Name { get; } = ServerName.From(nameof(PriceTrackerServer));
    public override Problem Solution => Problem.PriceTracker;

    protected override PriceTrackerClient CreateClient(TcpClient client, CancellationToken token)
        => new(client, token);
}