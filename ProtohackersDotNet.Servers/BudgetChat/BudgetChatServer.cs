using Microsoft.Extensions.Options;
using ProtoHackersDotNet.AsciiString;
using ProtoHackersDotNet.Helpers.ObservableTypes;

namespace ProtoHackersDotNet.Servers.BudgetChat;

public sealed class BudgetChatServer(IOptions<BudgetChatServerOptions> options) 
    : TcpServerBase<BudgetChatClient>, IServer<BudgetChatClient>
{
    public const byte LINE_DELIMITER = (byte) '\n';

    readonly int lineBufferLength = int.Min(1024, options.Value.MaxMessageLength);
    readonly ObservableStore<Guid, BudgetChatClient> joinedClients = new(client => client.Id);

    public override ServerName Name { get; } = ServerName.From(nameof(BudgetChatServer));

    public override Problem Solution { get; } = new(3, "BudgetChat");

    public override IObservable<string?> Status => this.joinedClients.CurrentCount.Select(count => $"Joined Clients: {count}");

    protected override BudgetChatClient CreateClient(TcpClient client, CancellationToken token)
    {
        BudgetChatClient newClient = new(this, client, token);
        _ = newClient.Joined.ContinueWith(JoinClient, CancellationToken.None);
        return newClient;

        void JoinClient(Task task)
        {
            Debug.Assert(task.IsCompletedSuccessfully);
            this.JoinClient(newClient);
        }
    }

    #region System notices

    internal ascii WelcomeMessage { get; } = options.Value.WelcomeMessage.TrimEnd().ToAscii() + LINE_DELIMITER;
    readonly ascii nameDelimiter = options.Value.NameDelimiter.ToAscii();
    readonly SystemNotice presenceNotice = SystemNotice.From(options.Value.PresenceNotice.TrimEnd());
    readonly SystemNotice joinNotice = SystemNotice.From(options.Value.JoinNotice.TrimEnd());
    readonly SystemNotice partNotice = SystemNotice.From(options.Value.PartNotice.TrimEnd());
    readonly ChatBroadcast chatNotice = ChatBroadcast.From("[{n}] ");

    /// <summary>Gets a formatted AsciiMessage detailing the presence joined users, omitting <paramref name="joiner"/>.</summary>
    /// <param name="joiner">The user who joined, and should be omitted from this message.</param>
    /// <returns>A properly formatted message, listing joined users omitting <paramref name="joiner"/>.</returns>
    internal ascii GetPresentNotice(BudgetChatClient joiner)
    {
        using ValueAsciiBuilder builder = new(this.lineBufferLength);
        builder.Append(presenceNotice.Value.Prefix);
        var users = this.joinedClients.Except(joiner).Select(connection => connection.UserName!.Value.Value);
        builder.AppendJoin(nameDelimiter, users);
        builder.Append(presenceNotice.Value.Postfix);
        builder.Append(LINE_DELIMITER);
        return builder.ToAscii();
    }

    #endregion System Notices

    /// <summary>Adds the client to the list of joined clients.</summary>
    /// <param name="client">The client to join.</param>
    void JoinClient(BudgetChatClient client) => this.joinedClients.Add(client);

    protected override Task OnClientDisconnect(BudgetChatClient client)
    {
        if (client.Joined.IsCompletedSuccessfully) this.joinedClients.Remove(client);
        return Task.CompletedTask;
    }

    #region Broadcasts

    /// <summary>Broadcasts the join message to all connected users except <paramref name="sender"/>.</summary>
    /// <param name="sender">The user who's join to announce.</param>
    /// <returns>A <see cref="Task"/> that indicates completion of the broadcast.</returns>
    async Task BroadcastMessage(BudgetChatClient sender, PrefixPostfixAscii format)
    {
        var message = FormatNotice(sender.UserName!.Value, format);
        await Broadcast(this.joinedClients.Except(sender), message.ToTransmission(true));
    }

    public async Task BroadcastJoin(BudgetChatClient joiner) => await BroadcastMessage(joiner, joinNotice.Value);
    public async Task BroadcastPart(BudgetChatClient leaver) => await BroadcastMessage(leaver, partNotice.Value);
    public async Task BroadcastChat(BudgetChatClient chatter, ReadOnlySequence<byte> message)
    {
        Guard.IsLessThanOrEqualTo(message.Length, options.Value.MaxMessageLength);
        Debug.Assert(chatter.UserName is not null);

        var chatMessage = FormatChat(chatter.UserName.Value, message);
        await Broadcast(this.joinedClients.Except(chatter), chatMessage.ToTransmission(true));
    }

    ascii FormatNotice(AsciiName name, PrefixPostfixAscii notice)
    {
        using ValueAsciiBuilder builder = new(stackalloc byte[this.lineBufferLength]);
        builder.Append(notice.Prefix);
        builder.Append(name.Value);
        builder.Append(notice.Postfix);
        builder.Append(LINE_DELIMITER);
        return builder.ToAscii();
    }

    ascii FormatChat(AsciiName name, ReadOnlySequence<byte> message)
    {
        using ValueAsciiBuilder builder = new(stackalloc byte[this.lineBufferLength]);
        builder.Append(this.chatNotice.Value.Prefix);
        builder.Append(name.Value);
        builder.Append(this.chatNotice.Value.Postfix);
        builder.Append(message);
        builder.Append(LINE_DELIMITER);
        return builder.ToAscii();
    }

    #endregion 

    const int MIN_NAME_LENGTH = 1;

    public AsciiName ValidateName(AsciiName name) => name switch {
        _ when (name is { Value.Length: int length } && length < MIN_NAME_LENGTH) || length > options.Value.MaxNameLength
            => ThrowFormatException<AsciiName>($"Length of name ({length}) is not within range [{MIN_NAME_LENGTH}, {options.Value.MaxNameLength}]"),
        _ when Clients.Any(client => client.UserName is AsciiName clientName && clientName == name)
            => ThrowFormatException<AsciiName>($"User with name \"{name}\" is already connected"),
        _ => name,
    };
}
