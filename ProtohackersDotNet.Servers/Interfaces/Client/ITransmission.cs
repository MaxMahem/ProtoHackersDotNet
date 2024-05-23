namespace ProtoHackersDotNet.Servers.Interfaces.Client;

public interface ITransmission
{
    ReadOnlyMemory<byte> Data { get; }
    string? Translation { get; }
    bool Broadcast { get; }
}