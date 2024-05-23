namespace ProtoHackersDotNet.Servers.Interfaces.Client.Messages;

public class Transmission(ReadOnlySequence<byte> data, string? translation, bool broadcast) : ITransmission
{
    public ReadOnlyMemory<byte> Data => data.ToArray();

    public string? Translation => translation;

    public bool Broadcast => broadcast;
}