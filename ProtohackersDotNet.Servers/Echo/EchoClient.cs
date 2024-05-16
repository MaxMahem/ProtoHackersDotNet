using ByteSizeLib;
using ProtoHackersDotNet.Servers.TcpServer;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ProtoHackersDotNet.Servers.Echo;

public class EchoClient(TcpClient client, EchoServer server, CancellationToken token)
    : TcpClientBase<EchoServer, EchoClient>(client, server, token)
{
    protected override SequencePosition? FindLineEnd(ReadOnlySequence<byte> buffer) => buffer.End;

    protected override async Task ProcessLine(ReadOnlySequence<byte> line) => await Transmit(line, false);

    protected override string TranslateReciept(ReadOnlySequence<byte> buffer) => ByteSize.FromBytes(buffer.Length).ToString();
}