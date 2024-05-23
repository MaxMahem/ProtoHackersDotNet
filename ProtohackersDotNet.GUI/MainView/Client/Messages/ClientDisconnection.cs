namespace ProtoHackersDotNet.GUI.MainView.Client.Messages;

public class ClientDisconnection(IServer server, IClient client) : ServerEvent(server)
{
    public override ServerEventType EventType => ServerEventType.ClientDisconnect;
    public override DisplayMessageType MessageType => DisplayMessageType.ClientDisconnect;
    public override string Message { get; } = $"Client from {client.EndPoint} disconnected.";
}