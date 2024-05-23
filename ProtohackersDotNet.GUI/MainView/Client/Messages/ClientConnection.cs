namespace ProtoHackersDotNet.GUI.MainView.Client.Messages;

public class ClientConnection(IServer server, IClient client) : ServerEvent(server)
{
    public override ServerEventType EventType => ServerEventType.ClientConnect;
    public override DisplayMessageType MessageType => DisplayMessageType.ClientConnect;
    public override string Message { get; } = $"Client from {client.EndPoint} connected.";
}