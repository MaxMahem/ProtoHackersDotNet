namespace ProtoHackersDotNet.Servers.Echo;

public class EchoServer : TcpServerBase<EchoClient>
{
    public override string Name => "Echo";
    public override int ProblemId => 0;

    protected override EchoClient CreateClient(TcpClient client, CancellationToken token) => new(this, client, token);
}