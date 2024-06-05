namespace ProtoHackersDotNet.Servers.Interface.Client.Messages;

public class Transmission : ITransmission
{
    public required ReadOnlyMemory<byte> Data { get; init; }
    public required string Translation { get; init; }
}