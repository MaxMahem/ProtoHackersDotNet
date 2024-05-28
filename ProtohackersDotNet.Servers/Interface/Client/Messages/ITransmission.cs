namespace ProtoHackersDotNet.Servers.Interface.Client.Messages;

public interface ITransmission
{
    ReadOnlyMemory<byte> Data { get; }
    string? Translation { get; }
    bool Broadcast { get; }
}