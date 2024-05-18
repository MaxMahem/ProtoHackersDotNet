using System.Text;

namespace ProtoHackersDotNet.Servers.BudgetChat;

public sealed class BudgetChatClient(TcpClient client, BudgetChatServer server, CancellationToken token)
    : TcpClientBase<BudgetChatServer, BudgetChatClient>(client, server, token)
{
    public BudgetChatClientState State { get; private set; } = BudgetChatClientState.Welcome;

    internal AsciiName? ChatName { get; private set; } = null;

    #region On Overloads

    protected override async Task OnConnect()
    {
        await Transmit(server.WelcomeMessageAscii.ToTransmission(false));
        State = BudgetChatClientState.Welcome;
    }

    protected override async Task OnException(Exception exception)
    {
        if (this.client.Connected)
            await Transmit(Encoding.ASCII.GetBytes(exception.Message + '\n'), exception.Message, false);
    }

    protected override async Task OnDisconnect()
    {
        if (State == BudgetChatClientState.Joined)
            await this.server.BroadcastPart(this);
        State = BudgetChatClientState.Parted;
    }

    #endregion

    protected override async Task ProcessLine(ReadOnlySequence<byte> line)
    {
        // trim the last character from the sequence, it's a line ending.
        line = line.Slice(0, line.Length - 1);
        switch (State) {
            case BudgetChatClientState.Welcome:
                ChatName = AsciiName.From(new(line));

                State = BudgetChatClientState.Joined;
                Status = $"Joined: {ChatName.Value}";

                await this.server.BroadcastJoin(this);

                await Transmit(this.server.GetPresentNotice(this).ToTransmission(false));
                break;
            case BudgetChatClientState.Joined:
                var chatMessage = this.server.FormatChatMessage(this, line);
                await this.server.BroadcastChat(this, chatMessage.ToTransmission(true));
                break;
            default:
                throw new InvalidOperationException();
        }
    }

    /// <summary>Finds the ending of the first line in <paramref name="buffer"/>, or <see langword="null""/> if absent.</summary>
    protected override SequencePosition? FindLineEnd(ReadOnlySequence<byte> buffer)
        => buffer.PositionOf(BudgetChatServer.LINE_DELIMITER, 1);

    protected override string TranslateReciept(ReadOnlySequence<byte> buffer)
        => Encoding.ASCII.GetString(buffer);
}