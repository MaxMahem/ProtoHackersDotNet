namespace ProtoHackersDotNet.Servers.Interface.Server.Events;

public class ClientConnectionEvent(IServer server, IClient client) : ServerEvent(server)
{
    public IClient Client { get; } = client;

    public override string Type { get; } = nameof(ClientConnectionEvent);

    public override MessageType MessageType => MessageType.Notice;

    public override string Message { get; } = $"Client connect from {client.ClientEndPoint}";
}