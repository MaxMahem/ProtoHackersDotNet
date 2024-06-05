using System.Collections.Concurrent;
using ProtoHackersDotNet.AsciiString;
using ProtoHackersDotNet.Helpers.ObservableTypes;

namespace ProtoHackersDotNet.Servers.BudgetChat;

public sealed class BudgetChatServer(BudgetChatServerOptions options) 
    : TcpServerBase<BudgetChatClient>, IServer<BudgetChatClient>
{
    public const byte LINE_DELIMITER = (byte) '\n';

    readonly int lineBufferLength = int.Min(1024, options.MaxMessageLength);
    readonly ObservableStore<BudgetChatClient, BudgetChatClient> joinedClients = new(client => client);

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
            this.joinedClients.Add(newClient);
        }
    }

    #region System notices

    internal ascii WelcomeMessage { get; } = options.WelcomeMessage.TrimEnd().ToAscii() + LINE_DELIMITER;
    readonly ascii nameDelimiter = options.NameDelimiter.ToAscii();
    readonly SystemNotice presenceNotice = SystemNotice.From(options.PresenceNotice.TrimEnd());
    readonly ascii emptyRoomNotice = options.EmptyRoomNotice.TrimEnd().ToAscii() + LINE_DELIMITER;
    readonly SystemNotice joinNotice = SystemNotice.From(options.JoinNotice.TrimEnd());
    readonly SystemNotice partNotice = SystemNotice.From(options.PartNotice.TrimEnd());
    readonly ChatBroadcast chatNotice = ChatBroadcast.From("[{n}] ");

    /// <summary>Gets a formatted AsciiMessage detailing the presence joined users, omitting <paramref name="joiner"/>.</summary>
    /// <param name="joiner">The user who joined, and should be omitted from this message.</param>
    /// <returns>A properly formatted message, listing joined users omitting <paramref name="joiner"/>.</returns>
    internal ascii GetPresentNotice(BudgetChatClient joiner)
    {
        if (this.joinedClients.Count is 0) return emptyRoomNotice;
        using ValueAsciiBuilder builder = new(this.lineBufferLength);
        builder.Append(presenceNotice.Value.Prefix);
        var users = this.joinedClients.Except(joiner).Select(connection => connection.UserName!.Value.Value);
        builder.AppendJoin(nameDelimiter, users);
        builder.Append(presenceNotice.Value.Postfix);
        builder.Append(LINE_DELIMITER);
        return builder.ToAscii();
    }

    #endregion System Notices

    protected override Task OnClientDisconnect(BudgetChatClient client)
    {
        if (client.Joined.IsCompletedSuccessfully) this.joinedClients.Remove(client);
        return Task.CompletedTask;
    }

    #region Broadcasts

    /// <summary>Broadcasts <paramref name="message"/> to <paramref name="recipients"/>.</summary>
    async Task Broadcast<TMessage>(IEnumerable<BudgetChatClient> recipients, TMessage message,
        CancellationToken token)
        where TMessage : ITransmission
    {
        ServerEventObservable.OnNext(DataBroadcastEvent.FromServer(message, this));
        await Task.WhenAll(recipients.Select(client => client.Transmit(message, token)));
    }

    /// <summary>Broadcasts a join message to all connected users except <paramref name="sender"/>.</summary>
    /// <param name="sender">The user who's send the message.</param>
    /// <param name="notice">The format of the message to send.</param>
    /// <returns>A <see cref="Task"/> that indicates completion of the broadcast.</returns>
    async Task BroadcastMessage(BudgetChatClient sender, SystemNotice notice, CancellationToken token)
    {
        var message = FormatNotice(sender.UserName!.Value, notice);
        await Broadcast(this.joinedClients.Except(sender), new AsciiTransmission(message), token);
    }

    public async Task BroadcastJoin(BudgetChatClient joiner, CancellationToken token) 
        => await BroadcastMessage(joiner, joinNotice, token);
    public async Task BroadcastPart(BudgetChatClient leaver, CancellationToken token) 
        => await BroadcastMessage(leaver, partNotice, token);
    public async Task BroadcastChat(BudgetChatClient chatter, ascii message, CancellationToken token)
    {
        Guard.IsLessThanOrEqualTo(message.Length, options.MaxMessageLength);
        Debug.Assert(chatter.UserName is not null);

        var chatMessage = FormatChat(chatter.UserName.Value, message);
        await Broadcast(this.joinedClients.Except(chatter), new AsciiTransmission(chatMessage), token);
    }

    ascii FormatNotice(AsciiName name, SystemNotice notice)
    {
        using ValueAsciiBuilder builder = new(stackalloc byte[this.lineBufferLength]);
        builder.Append(notice.Value.Prefix);
        builder.Append(name.Value);
        builder.Append(notice.Value.Postfix);
        builder.Append(LINE_DELIMITER);
        return builder.ToAscii();
    }

    ascii FormatChat(AsciiName name, ascii message)
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
        _ when (name is { Value.Length: int length } && length < MIN_NAME_LENGTH) || length > options.MaxNameLength
            => InvalidNameException.Throw<AsciiName>($"Length of name ({length}) is not within range [{MIN_NAME_LENGTH}, {options.MaxNameLength}]"),
        _ when Clients.Any(client => client.UserName is AsciiName clientName && clientName == name)
            => InvalidNameException.Throw<AsciiName>($"User with name \"{name}\" is already connected"),
        _ => name,
    };
}

public class SemaphoreQueue(int initialCount, int maxCount = SemaphoreQueue.NO_MAXIMUM)
{
    public const int NO_MAXIMUM = int.MaxValue;

    readonly SemaphoreSlim semaphore = new(initialCount, maxCount);
    readonly ConcurrentQueue<TaskCompletionSource<bool>> taskQueue = new();
    
    public Task WaitAsync()
    {
        var tcs = new TaskCompletionSource<bool>();
        this.taskQueue.Enqueue(tcs);
        return this.semaphore.WaitAsync().ContinueWith(task => {
            if (taskQueue.TryDequeue(out var popped))
                popped.SetResult(true);
        });
    }

    public void Release() => this.semaphore.Release();
}