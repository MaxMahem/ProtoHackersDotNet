namespace ProtoHackersDotNet.Servers.Interface.Server.Events;

public class ClientConnectionEvent(IServer server, IClient client) : ServerEvent(server)
{
    public IClient Client { get; } = client;
    public override ServerEventType EventType => ServerEventType.ClientConnect;

    public override string Type { get; } = ServerEventType.ClientConnect.ToString();

    public override MessageCategory Category => MessageCategory.StatusChange;

    public override string Message { get; } = $"Client connect from {client.ClientEndPoint}";
}