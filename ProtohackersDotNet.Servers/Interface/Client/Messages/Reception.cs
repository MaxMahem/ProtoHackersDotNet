namespace ProtoHackersDotNet.Servers.Interface.Client.Messages;

public class Reception(ReadOnlySequence<byte> data, string? translation) : ITransmission
{
    public ReadOnlyMemory<byte> Data => data.ToArray();

    public string? Translation => translation;

    public bool Broadcast => false;
}