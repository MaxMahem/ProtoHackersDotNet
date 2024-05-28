namespace ProtoHackersDotNet.Servers.Interface.Server.Events;

public class ClientDisconnectEvent(IServer server, IClient client) : ServerEvent(server)
{
    public IClient Client { get; } = client;

    public override ServerEventType EventType => ServerEventType.ClientDisconnect;

    public override string Type { get; } = ServerEventType.ClientDisconnect.ToString();

    public override MessageCategory Category => MessageCategory.StatusChange;

    public override string Message { get; } = $"Client disconnect from {client.ClientEndPoint}";
}