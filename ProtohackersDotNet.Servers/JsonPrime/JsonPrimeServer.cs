namespace ProtoHackersDotNet.Servers.JsonPrime;

public sealed class JsonPrimeServer : TcpServerBase<JsonPrimeClient>
{
    public override string Name => "JsonPrime";

    public override int ProblemId => 1;

    protected override JsonPrimeClient CreateClient(TcpClient client, CancellationToken token) 
        => new(this, client, token);
}