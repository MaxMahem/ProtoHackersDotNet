using Microsoft.Extensions.Options;
using ProtoHackersDotNet.AsciiString;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ProtoHackersDotNet.Servers.BudgetChat;

public sealed partial class BudgetChatServer(IOptions<BudgetChatServerOptions> options) 
    : TcpServerBase<BudgetChatClient>, IServer<BudgetChatClient>
{
    public const byte LINE_DELIMITER = (byte) '\n';
    public const byte SPACE_DELIMITER = (byte) ' ';
    public const byte SYSTEM_NOTICE_START_TOKEN = (byte) '*';

    int lineBufferLength = int.Min(1024, options.Value.MaxMessageLength);

    public override Problem Problem { get; } = new(3, "BudgetChat");

    protected override BudgetChatClient CreateClient(TcpClient client, CancellationToken token) 
        => new(this, client, token);

    #region System notices

    internal ascii WelcomeMessage { get; } = options.Value.WelcomeMessage.TrimEnd().ToAscii() + LINE_DELIMITER;
    readonly ascii nameDelimiter = options.Value.NameDelimiter.ToAscii();
    readonly SystemNotice presenceNotice = SystemNotice.From(options.Value.PresenceNotice.TrimEnd());
    readonly SystemNotice joinNotice = SystemNotice.From(options.Value.JoinNotice.TrimEnd());
    readonly SystemNotice partNotice = SystemNotice.From(options.Value.PartNotice.TrimEnd());
    readonly PrefixPostfixAscii chatNotice = PrefixPostfixAscii.From("[{n}] ", ChatBroadcastRegex());

    [GeneratedRegex(@"(^\[){n}(] )$")]
    private static partial Regex ChatBroadcastRegex();

    /// <summary>Gets a formatted AsciiMessage detailing the presence joined users, omitting <paramref name="joiner"/>.</summary>
    /// <param name="joiner">The user who joined, and should be omitted from this message.</param>
    /// <returns>A properly formatted message, listing joined users omitting <paramref name="joiner"/>.</returns>
    internal ascii GetPresentNotice(BudgetChatClient joiner)
    {
        using ValueAsciiBuilder builder = new(this.lineBufferLength);
        builder.Append(presenceNotice.Value.Prefix);
        var users = JoinedClients.Except(joiner).Select(connecetion => connecetion.UserName!.Value.Value);
        builder.AppendJoin(nameDelimiter, users);
        builder.Append(presenceNotice.Value.Postfix);
        builder.Append(LINE_DELIMITER);
        return builder.ToAscii();
    }

    #endregion System Notices

    /// <summary>An enumeration of all joined users.</summary>
    public IEnumerable<BudgetChatClient> JoinedClients
        => Clients.Where(client => client.State is BudgetChatClientState.Joined);

    #region Broadcasts

    /// <summary>Broadcasts the join message to all connected users except <paramref name="sender"/>.</summary>
    /// <param name="sender">The user who's join to announce.</param>
    /// <returns>A <see cref="Task"/> that indicates completion of the broadcast.</returns>
    async Task BroadcastMessage(BudgetChatClient sender, PrefixPostfixAscii format)
    {
        var message = FormatNotice(sender.UserName!.Value, format);
        await Broadcast(JoinedClients.Except(sender), message.ToTransmission(true));
    }

    public async Task BroadcastJoin(BudgetChatClient joiner) => await BroadcastMessage(joiner, joinNotice.Value);
    public async Task BroadcastPart(BudgetChatClient leaver) => await BroadcastMessage(leaver, partNotice.Value);
    public async Task BroadcastChat(BudgetChatClient chatter, ReadOnlySequence<byte> message)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(message.Length, options.Value.MaxMessageLength);
        ArgumentNullException.ThrowIfNull(chatter.UserName);
        var chatMessage = FormatChat(chatter.UserName.Value, message);
        await Broadcast(JoinedClients.Except(chatter), chatMessage.ToTransmission(true));
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
        builder.Append(this.chatNotice.Prefix);
        builder.Append(name.Value);
        builder.Append(this.chatNotice.Postfix);
        builder.Append(message);
        builder.Append(LINE_DELIMITER);
        return builder.ToAscii();
    }

    #endregion 

    const int MIN_NAME_LENGTH = 1;

    public AsciiName ValidateName(AsciiName name)
    {
        Guard.IsBetweenOrEqualTo(name.Value.Length, MIN_NAME_LENGTH, options.Value.MaxNameLength);

        return Clients.Any(client => client.UserName is AsciiName clientName && clientName.Value == name)
            ? ThrowHelper.ThrowInvalidOperationException<AsciiName>($"User with name \"{name}\" is already connected.\n")
            : name;
    }
}