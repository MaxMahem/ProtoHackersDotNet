using ByteSizeLib;
using ProtoHackersDotNet.Servers.Helpers;

namespace ProtoHackersDotNet.GUI.MainView.Client.Messages;

public class ClientTransmission(IClient client, ITransmission transmission) : ClientEvent(client)
{
    public bool Broadcast { get; } = transmission.Broadcast;
    public override DisplayMessageType MessageType => Broadcast ? DisplayMessageType.ClientBroadcast
                                                                : DisplayMessageType.ClientTransmission;
    public override string Message { get; } = transmission.Translation
        ?? (transmission.Broadcast ? $"{transmission.Data.ToByteSize()} broadcast."
                                   : $"{transmission.Data.ToByteSize()} transmitted.");
    public ByteSize BytesTransmitted { get; } = transmission.Data.ToByteSize();
}