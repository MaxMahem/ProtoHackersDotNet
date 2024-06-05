namespace ProtoHackersDotNet.Servers.UdpDatabase;

public class UdpTransmission(ReadOnlyMemory<byte> data) : ITransmission
{
    public ReadOnlyMemory<byte> Data { get; } = data;
    public string Translation { get; } = Encoding.ASCII.GetString(data.Span);

    public static UdpTransmission FromReception(UdpReceiveResult result)
        => new(result.Buffer);

    public static UdpTransmission FromData(ReadOnlyMemory<byte> data) => new(data);
}