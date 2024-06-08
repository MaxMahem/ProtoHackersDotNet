namespace ProtoHackersDotNet.Servers.Echo;

public class EchoServer : TcpServerBase<EchoClient>
{
    public override ServerName Name { get; } = ServerName.From(nameof(EchoServer));

    public override Problem Solution => Problem.Echo;

    protected override EchoClient CreateClient(TcpClient client, CancellationToken token) => new(client, token);
}