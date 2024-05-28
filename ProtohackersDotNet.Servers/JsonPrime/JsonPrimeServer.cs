namespace ProtoHackersDotNet.Servers.JsonPrime;

public sealed class JsonPrimeServer : TcpServerBase<JsonPrimeClient>
{
    public override ServerName Name { get; } = ServerName.From(nameof(JsonPrimeServer));
    public override Problem Problem { get; } = new(1, "JsonPrime");

    protected override JsonPrimeClient CreateClient(TcpClient client, CancellationToken token) 
        => new(this, client, token);
}