namespace ProtoHackersDotNet.Servers.Echo;

public class EchoClient(EchoServer server, TcpClient client, CancellationToken token)
    : TcpClientBase<EchoServer>(server, client, token)
{
    public static EchoClient Create(EchoServer server, TcpClient client, CancellationToken token) 
        => new(server, client, token);
    protected override SequencePosition? FindLineEnd(ReadOnlySequence<byte> buffer) => buffer.End;

    protected override async Task ProcessLine(ReadOnlySequence<byte> line) => await Transmit(line, false);

    protected override string TranslateRecieption(ReadOnlySequence<byte> buffer) => $"{buffer.ToByteSize()} recieved.";
}