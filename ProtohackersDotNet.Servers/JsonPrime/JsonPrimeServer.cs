namespace ProtoHackersDotNet.Servers.JsonPrime;

public sealed class JsonPrimeServer : TcpServerBase<JsonPrimeClient>
{
    public override ServerName Name { get; } = ServerName.From(nameof(JsonPrimeServer));
    public override Problem Solution => Problem.JsonPrime;

    protected override JsonPrimeClient CreateClient(TcpClient client, CancellationToken token) 
        => new(client, token);
}