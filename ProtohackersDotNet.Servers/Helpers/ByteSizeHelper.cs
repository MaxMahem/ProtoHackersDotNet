namespace ProtoHackersDotNet.Servers.Helpers;

public static class ByteSizeHelper
{
    public static ByteSize ToByteSize(this ReadOnlySequence<byte> data) => ByteSize.FromBytes(data.Length);
    public static ByteSize ToByteSize(this ReadOnlyMemory<byte> data) => ByteSize.FromBytes(data.Length);
}