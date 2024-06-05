namespace ProtoHackersDotNet.Servers.Echo;

public class EchoClient(TcpClient client, CancellationToken token)
    : TcpClientBase(client, token)
{
    protected override SequencePosition? FindLineEnd(ReadOnlySequence<byte> buffer) => buffer.End;

    protected override async Task ProcessLine(ReadOnlySequence<byte> line, CancellationToken token) 
        => await Transmit(line, token);

    protected override string TranslateReception(ReadOnlySequence<byte> buffer) => $"{buffer.ToByteSize()} received";
}