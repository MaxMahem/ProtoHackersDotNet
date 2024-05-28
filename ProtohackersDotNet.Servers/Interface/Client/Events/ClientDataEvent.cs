using ProtoHackersDotNet.Servers.Interface.Client.Messages;

namespace ProtoHackersDotNet.Servers.Interface.Client.Events;

public abstract class ClientDataEvent(IClient client, ITransmission transmission) : ClientEvent(client)
{
    public ITransmission Transmission { get; } = transmission;
    public ByteSize MessageSize { get; } = transmission.Data.ToByteSize();
    public override MessageCategory Category => MessageCategory.Data;
}
