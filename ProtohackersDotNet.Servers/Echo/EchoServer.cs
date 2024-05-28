namespace ProtoHackersDotNet.Servers.Echo;

public class EchoServer : TcpServerBase<EchoClient>
{
    public override ServerName Name { get; } = ServerName.From(nameof(EchoServer));

    public override Problem Problem { get; } = new(0, "Echo");

    protected override EchoClient CreateClient(TcpClient client, CancellationToken token) => new(this, client, token);
}