using ProtoHackersDotNet.AsciiString;

namespace ProtoHackersDotNet.Servers.BudgetChat;

public sealed class BudgetChatClient(BudgetChatServer server, TcpClient client, CancellationToken token)
    : TcpClientBase<BudgetChatServer>(server, client, token)
{
    readonly static byte[] WhiteSpace = [(byte) ' ', (byte) '\n'];

    public BudgetChatClientState State { get; private set; } = BudgetChatClientState.Welcome;

    public AsciiName? UserName { get; private set; } = null;

    #region On Overloads

    protected override async Task OnConnect(CancellationToken token)
    {
        await Transmit(Server.WelcomeMessage.ToTransmission(false));
        State = BudgetChatClientState.Welcome;
    }

    protected override async Task OnException(Exception exception, CancellationToken token)
    {
        if (LatestConnectionStatus is Interface.Client.ConnectionStatus.Connected) {
            ascii ascii = new(exception.Message + '\n');
            await Transmit(ascii.ToTransmission(false));
        }
            
    }

    protected override async Task OnDisconnect(CancellationToken token)
    {
        if (State == BudgetChatClientState.Joined)
            await Server.BroadcastPart(this);
        State = BudgetChatClientState.Parted;
    }

    #endregion

    protected override async Task ProcessLine(ReadOnlySequence<byte> line)
    {
        try {
            // Trim any trailing whitespace from the sequence. 
            var lastPosition = line.LastPositionOfAnyExcept(WhiteSpace)
            ?? ThrowHelper.ThrowInvalidOperationException<SequencePosition>("There should always be a line ending to trim");
            line = line.Slice(line.Start, line.GetPosition(1, lastPosition)); // slice is *exclusive* of the end, we want inclusive.

            ascii asciiLine = new(line);

            switch (State) {
                case BudgetChatClientState.Welcome:
                    var userName = AsciiName.From(asciiLine);
                    UserName = Server.ValidateName(userName);

                    State = BudgetChatClientState.Joined;
                    LatestStatus = $"Joined: {UserName}";

                    await Server.BroadcastJoin(this);

                    await Transmit(Server.GetPresentNotice(this).ToTransmission(false));
                    break;
                case BudgetChatClientState.Joined:
                    await Server.BroadcastChat(this, line);
                    break;
                default: throw new InvalidOperationException();
            }
        }
        catch (Exception exception) when (exception is not InvalidOperationException) {
            throw new ClientException(exception) { Client = this };
        }
    }

    /// <summary>Finds the ending of the first line in <paramref name="buffer"/>, or <see langword="null""/> if absent.</summary>
    protected override SequencePosition? FindLineEnd(ReadOnlySequence<byte> buffer)
        => buffer.PositionOf(BudgetChatServer.LINE_DELIMITER, 1);

    protected override string TranslateReception(ReadOnlySequence<byte> buffer)
        => Encoding.ASCII.GetString(buffer);
}
