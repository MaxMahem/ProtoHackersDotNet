namespace ProtoHackersDotNet.Servers.Interface.Server.Events;

public class ClientDisconnectEvent(IServer server, IClient client) : ServerEvent(server)
{
    public IClient Client { get; } = client;

    public override string Type { get; } = nameof(ClientDisconnectEvent);

    public override MessageType MessageType => MessageType.Notice;

    public override string Message { get; } = $"Client disconnect from {client.ClientEndPoint}";
}