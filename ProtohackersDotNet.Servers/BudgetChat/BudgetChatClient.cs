using ProtoHackersDotNet.AsciiString;
using IConnectionStatus = ProtoHackersDotNet.Servers.Interface.Client.ConnectionStatus;
using ProtoHackersDotNet.Helpers.ObservableTypes;

namespace ProtoHackersDotNet.Servers.BudgetChat;

public sealed class BudgetChatClient(BudgetChatServer server, TcpClient client, CancellationToken token)
    : TcpClientBase(client, token)
{
    readonly static ImmutableArray<byte> WhiteSpace = [(byte) ' ', (byte) '\n', (byte) '\r'];

    readonly ObservableValue<BudgetChatClientState> stateObserver = new(BudgetChatClientState.Welcome);

    public AsciiName? UserName { get; private set; } = null;

    readonly TaskCompletionSource joinedCompleteSource = new();
    public Task Joined => this.joinedCompleteSource.Task;

    public override IObservable<string?> Status => this.stateObserver.Values.Select(GetStatusFromState);

    string GetStatusFromState(BudgetChatClientState state) => UserName is not null ? $"{state}: '{UserName}'" : state.ToString();

    #region On Overloads

    protected override async Task OnConnect(CancellationToken token)
    {
        await Transmit(new AsciiTransmission(server.WelcomeMessage), token);
        this.stateObserver.LatestValue = BudgetChatClientState.Welcome;
    }

    protected override async Task OnException(Exception exception, CancellationToken token)
    {
        if (LatestConnectionStatus is IConnectionStatus.Connected) {
            ascii ascii = new(exception.Message + '\n');
            await Transmit(new AsciiTransmission(ascii), token);
        }

    }

    protected override async Task OnDisconnect(CancellationToken token)
    {
        if (this.stateObserver.LatestValue == BudgetChatClientState.Joined)
            await server.BroadcastPart(this, token);
        this.stateObserver.LatestValue = BudgetChatClientState.Parted;
    }
    #endregion

    static readonly ReadOnlySequence<byte> BlankLine = new([BudgetChatServer.LINE_DELIMITER]);

    protected override async Task ProcessLine(ReadOnlySequence<byte> line, CancellationToken token)
    {
        try {
            ascii asciiLine = TrimInput(line);

            switch (this.stateObserver.LatestValue) {
                case BudgetChatClientState.Welcome:
                    var userName = AsciiName.From(asciiLine);
                    UserName = server.ValidateName(userName);

                    await JoinClient();
                    break;
                case BudgetChatClientState.Joined:
                    await server.BroadcastChat(this, asciiLine, token);
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }
        catch (Exception exception) when (exception is not InvalidOperationException) {
            ClientException.ReThrow(exception, this);
        }
    }

    /// <summary>Trims any trailing <see cref="WhiteSpace"/> from <paramref name="line"/>.</summary>
    /// <param name="line">The input to trim.</param>
    /// <returns><paramref name="line"/> turned into <see cref="ascii"/> and trimmed of any trailing <see cref="WhiteSpace"/>.</returns>
    private static ascii TrimInput(ReadOnlySequence<byte> line)
    {
        line = line.LastPositionOfAnyExcept(WhiteSpace.AsSpan()) is SequencePosition lastPosition
            ? line.Slice(line.Start, line.GetPosition(1, lastPosition)) // slice inclusive of last position.
            : BlankLine; // returning null means the sequence must be only whitespace. Use a pre-blanked line.
        return new (line);
    }

    async Task JoinClient()
    {
        await Transmit(new AsciiTransmission(server.GetPresentNotice(this)));

        this.stateObserver.LatestValue = BudgetChatClientState.Joined;
        this.joinedCompleteSource.SetResult();
        await server.BroadcastJoin(this, token);
    }

    /// <summary>Finds the ending of the first line in <paramref name="buffer"/>, or <see langword="null""/> if absent.</summary>
    protected override SequencePosition? FindLineEnd(ReadOnlySequence<byte> buffer)
        => buffer.PositionOf(BudgetChatServer.LINE_DELIMITER, 1);

    protected override string TranslateReception(ReadOnlySequence<byte> buffer)
        => Encoding.ASCII.GetString(buffer);

    enum BudgetChatClientState { Welcome, Joined, Parted }
}