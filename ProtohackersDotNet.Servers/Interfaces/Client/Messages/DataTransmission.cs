namespace ProtoHackersDotNet.Servers.Interfaces.Client.Messages;

public class DataTransmission(IClient client, string message) : ClientEvent(client)
{
    public required bool Broadcast { get; init; }
    public override DisplayMessageType MessageType => Broadcast ? DisplayMessageType.ClientBroadcast
                                                                : DisplayMessageType.ClientTransmission;
    public override string Message { get; } = message;
    public required ByteSize BytesTransmitted { get; init; }
}