namespace ProtoHackersDotNet.Servers.PriceTracker;

public sealed class PriceTrackerServer : TcpServerBase<PriceTrackerClient>
{
    public override string Name => "PriceTracker";

    public override int ProblemId => 2;

    protected override PriceTrackerClient CreateClient(TcpClient client, CancellationToken token)
        => new(this, client, token);
}