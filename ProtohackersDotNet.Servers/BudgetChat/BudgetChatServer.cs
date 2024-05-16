using System.IO;
using CommunityToolkit.Diagnostics;
using ProtoHackersDotNet.AsciiString;

namespace ProtoHackersDotNet.Servers.BudgetChat;

public sealed class BudgetChatServer(IPAddress address, ushort port)
    : TcpServer<BudgetChatServer, BudgetChatClient>(address, port), IServer<BudgetChatClient>
{
    public const byte LINE_DELIMITER = (byte) '\n';
    public const byte SYSTEM_NOTICE_START_TOKEN = (byte) '*';

    public static string ServerName => "BudgetChat";
    public static int ProblemId => 3;

    protected override BudgetChatServer Instance => this;

    public string WelcomeMessage { init => WelcomeMessageAscii = new(value); }
    internal Ascii WelcomeMessageAscii { get; private init; } = new("Welcome to budget chat, please enter your name: \n" );

    #region system notices

    public string NameDelimiter { init => nameDelimiterAscii = new(value); }
    Ascii nameDelimiterAscii = new(", ");

    /// <summary>Prefix of the notice detailing the joined users in the chat. Must begin with '*".</summary>
    public BudgetChatSystemNotice PresenceNotice { get; init; } = "* Present in the room: ";
    
    /// <summary>Gets a formatted AsciiMessage detailing the presence joined users, omitting <paramref name="joiner"/>.</summary>
    /// <param name="joiner">The user who joined, and should be omitted from this message.</param>
    /// <returns>A properly formatted message, listing joined users omitting <paramref name="joiner"/>.</returns>
    internal Ascii GetPresentNotice(BudgetChatClient joiner)
    {
        using ValueAsciiBuilder builder = new();
        builder.Append(PresenceNotice.Value);
        var users = JoinedUsers.Except(joiner).Select(connecetion => connecetion.ChatName!.Value.Value);
        builder.AppendJoin(nameDelimiterAscii, users);
        builder.Append((byte) '\n');
        return builder.ToAscii();
    }

    /// <summary>Prefix of the notice to display when a user joins the chat. Must begin with '*'.</summary>
    public BudgetChatSystemNotice JoinNotice { get; init; } = "* Joins the chat: ";

    /// <summary>Gets a formated message announcing the join of <paramref name="joiner"/>.</summary>
    /// <param name="joiner">The user joining the chat.</param>
    /// <returns>A formated chat message announcing the join of <paramref name="joiner"/>.</returns>
    Ascii GetJoinMessage(BudgetChatClient joiner)
    {
        using ValueAsciiBuilder builder = new();
        builder.Append(JoinNotice.Value);
        builder.Append(joiner.ChatName!.Value.Value);
        builder.Append((byte) '\n');
        return builder.ToAscii();
    }

    /// <summary>An enumeration of all joined users.</summary>
    public IEnumerable<BudgetChatClient> JoinedUsers
        => Clients.Where(client => client.State is BudgetChatClientState.Joined);

    /// <summary>Broadcasts the join message to all connected users except <paramref name="joiner"/>.</summary>
    /// <param name="joiner">The user who's join to announce.</param>
    /// <returns>A <see cref="Task"/> that indicates completion of the broadcast.</returns>
    public async Task BroadcastJoin(BudgetChatClient joiner) 
        => await Broadcast(JoinedUsers.Except(joiner), GetJoinMessage(joiner).ToTransmission(true));

    public async Task BroadcastChat(BudgetChatClient chatter, AsciiTransmission message)
        => await Broadcast(JoinedUsers.Except(chatter), message);

    /// <summary>Prefix of the notice to display when a user leaves the chat. Must begin with '*'.</summary>
    public BudgetChatSystemNotice PartNotice { get; init; } = "* Leaves the chat: ";

    Ascii GetPartMessage(BudgetChatClient leaver)
    {
        using ValueAsciiBuilder builder = new();
        builder.Append(PartNotice.Value);
        builder.Append(leaver.ChatName!.Value.Value);
        builder.Append((byte) '\n');
        return builder.ToAscii();
    }

    public async Task BroadcastPart(BudgetChatClient leaver)
        => await Broadcast(JoinedUsers.Except(leaver), GetPartMessage(leaver).ToTransmission(true));

    #endregion System Notices

    public int MaxNameLength
    {
        private get => maxNameLength;
        init
        {
            Guard.IsGreaterThan(value, MIN_MAX_NAME_LENGTH);
            maxNameLength = value;
        }
    }
    int maxNameLength = 16;
    const int MIN_MAX_NAME_LENGTH = 16;
    const int MIN_NAME_LENGTH = 1;

    // public Ascii ValidateName(ReadOnlySequence<byte> nameSequence)
    // {
    //     Ascii name = new(nameSequence);
    //     for (int index = 0; index < name.Length; index++) {
    //         if (!char.IsAsciiLetterOrDigit((char) name[index]))
    //             throw new InvalidDataException("Non alphanumeric character found.");
    //     }
    // 
    //     Guard.IsBetweenOrEqualTo(nameSequence.Length, MIN_NAME_LENGTH, MaxNameLength);
    // 
    //     return Clients.Any(client => client.ChatName == name)
    //         ? ThrowFormatException<Ascii>($"User with name \"{name}\" is already connected.\n")
    //         : name;
    // }

    public int MaxMessageLength
    {
        private get => maxMessageLength;
        init
        {
            Guard.IsGreaterThan(value, MIN_MAX_MESSAGE_LENGTH);
            maxMessageLength = value;
        }
    }

    int maxMessageLength = MIN_MAX_MESSAGE_LENGTH;
    const int MIN_MAX_MESSAGE_LENGTH = 1000;

    static readonly Ascii ChatBroadcastNamePrefix = new("[");
    static readonly Ascii ChatBroadcastNamePostfix = new("] ");

    internal Ascii FormatChatMessage(BudgetChatClient sender, ReadOnlySequence<byte> chatSequence)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(chatSequence.Length, MaxMessageLength);

        using ValueAsciiBuilder builder = new(maxMessageLength);

        builder.Append(ChatBroadcastNamePrefix);
        builder.Append(sender.ChatName!.Value.Value);
        builder.Append(ChatBroadcastNamePostfix);
        builder.Append(chatSequence);
        builder.Append(LINE_DELIMITER);

        return builder.ToAscii();
    }

    public static IServer<BudgetChatClient> Create(IPAddress address, ushort port) => new BudgetChatServer(address, port);
    protected override BudgetChatClient CreateClient(TcpClient client, CancellationToken token)
        => new(client, this, token);

    public static string Description => descriptionData.Value;
    static readonly string descriptionPath = Path.Combine(ServerName, ServerName + ".md");
    static readonly Lazy<string> descriptionData = new(() => File.ReadAllText(descriptionPath));
}