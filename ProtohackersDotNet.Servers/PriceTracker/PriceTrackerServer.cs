namespace ProtoHackersDotNet.Servers.PriceTracker;

public sealed class PriceTrackerServer : TcpServerBase<PriceTrackerClient>
{
    public override ServerName Name { get; } = ServerName.From(nameof(PriceTrackerServer));
    public override Problem Problem { get; } = new(2, "PriceTracker");

    protected override PriceTrackerClient CreateClient(TcpClient client, CancellationToken token)
        => new(this, client, token);
}