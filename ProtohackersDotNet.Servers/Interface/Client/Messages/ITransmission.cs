namespace ProtoHackersDotNet.Servers.Interface.Client.Messages;

public interface ITransmission
{
    ReadOnlyMemory<byte> Data { get; }
    string Translation { get; }
    ByteSize BytesTransmitted => ByteSize.FromBytes(Data.Length);
}