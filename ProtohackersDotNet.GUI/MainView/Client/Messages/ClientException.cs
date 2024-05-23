namespace ProtoHackersDotNet.GUI.MainView.Client.Messages;

public class ClientException(IClient client, Exception exception) : ClientEvent(client)
{
    public override DisplayMessageType MessageType => DisplayMessageType.ClientException;
    public override string Message { get; } = exception.Message;
}