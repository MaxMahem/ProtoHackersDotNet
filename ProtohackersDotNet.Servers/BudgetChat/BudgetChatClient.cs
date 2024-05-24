using System.Text;
using ProtoHackersDotNet.AsciiString;

namespace ProtoHackersDotNet.Servers.BudgetChat;

public sealed class BudgetChatClient(BudgetChatServer server, TcpClient client, CancellationToken token)
    : TcpClientBase<BudgetChatServer>(server, client, token)
{
    readonly static byte[] WhiteSpace = [(byte) ' ', (byte) '\n'];

    public BudgetChatClientState State { get; private set; } = BudgetChatClientState.Welcome;

    public AsciiName? UserName { get; private set; } = null;

    #region On Overloads

    protected override async Task OnConnect()
    {
        await Transmit(Server.WelcomeMessage.ToTransmission(false));
        State = BudgetChatClientState.Welcome;
    }

    protected override async Task OnException(Exception exception)
    {
        if (CurrentConnectionStatus is Interfaces.Client.ConnectionStatus.Connected) {
            ascii ascii = new(exception.Message + '\n');
            await Transmit(ascii.ToTransmission(false));
        }
            
    }

    protected override async Task OnDisconnect()
    {
        if (State == BudgetChatClientState.Joined)
            await Server.BroadcastPart(this);
        State = BudgetChatClientState.Parted;
    }

    #endregion

    protected override async Task ProcessLine(ReadOnlySequence<byte> line)
    {
        // Trim any trailing whitespace from the sequence. 
        var lastPosition = line.LastPositionOfAnyExcept(WhiteSpace) 
            ?? ThrowHelper.ThrowInvalidOperationException<SequencePosition>("There should always be a line ending to trim.");
        line = line.Slice(line.Start, line.GetPosition(1, lastPosition)); // slice is *exclusive* of the end, we want inclusive.

        switch (State) {
            case BudgetChatClientState.Welcome:
                UserName = AsciiName.From(new(line));

                State = BudgetChatClientState.Joined;
                StatusValue = $"Joined: {UserName.Value}";

                await Server.BroadcastJoin(this);

                await Transmit(Server.GetPresentNotice(this).ToTransmission(false));
                break;
            case BudgetChatClientState.Joined:
                await Server.BroadcastChat(this, line);
                break;
            default:
                throw new InvalidOperationException();
        }
    }

    /// <summary>Finds the ending of the first line in <paramref name="buffer"/>, or <see langword="null""/> if absent.</summary>
    protected override SequencePosition? FindLineEnd(ReadOnlySequence<byte> buffer)
        => buffer.PositionOf(BudgetChatServer.LINE_DELIMITER, 1);

    protected override string TranslateRecieption(ReadOnlySequence<byte> buffer)
        => Encoding.ASCII.GetString(buffer);
}
