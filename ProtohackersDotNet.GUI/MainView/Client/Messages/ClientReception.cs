using ByteSizeLib;
using ProtoHackersDotNet.Servers.Helpers;

namespace ProtoHackersDotNet.GUI.MainView.Client.Messages;

public class ClientReception(IClient client, ITransmission transmission) : ClientEvent(client)
{
    public bool Broadcast { get; } = transmission.Broadcast;
    public override DisplayMessageType MessageType => DisplayMessageType.ClientRecieption;
    public override string Message { get; } = transmission.Translation ?? $"{transmission.Data.ToByteSize()} recieved.";
    public ByteSize BytesRecieved { get; } = transmission.Data.ToByteSize();
}
