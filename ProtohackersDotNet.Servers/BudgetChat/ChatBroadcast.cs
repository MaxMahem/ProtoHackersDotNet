using Vogen;

namespace ProtoHackersDotNet.Servers.BudgetChat;

[ValueObject<PrefixPostfixAscii>]
[SuppressMessage("Usage", "AddValidationMethod:Value Objects can have validation", Justification = "From includes validation")]
public readonly partial struct ChatBroadcast
{
    public static ChatBroadcast From(string notice)
    {
        var prefixPostfix = PrefixPostfixAscii.From(notice, ChatBroadcastRegex());
        return From(prefixPostfix);
    }

    [GeneratedRegex(@"(^\[){n}(] )$")]
    private static partial Regex ChatBroadcastRegex();
}
