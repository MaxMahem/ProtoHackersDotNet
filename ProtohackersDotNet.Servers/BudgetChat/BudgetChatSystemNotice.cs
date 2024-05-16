using Vogen;
using ProtoHackersDotNet.AsciiString;

namespace ProtoHackersDotNet.Servers.BudgetChat;

[ValueObject<Ascii>]
public readonly partial struct BudgetChatSystemNotice
{
    public const byte SYSTEM_NOTICE_START_TOKEN = (byte) '*';

    public static Validation Validate(Ascii notice) =>
          notice.Length > 0 && notice[0] is SYSTEM_NOTICE_START_TOKEN ? Validation.Ok 
            : Validation.Invalid($"Notice must start with `{SYSTEM_NOTICE_START_TOKEN}`.");

    public static implicit operator BudgetChatSystemNotice(string input)
    {
        Ascii ascii = new Ascii(input);
        return From(ascii);
    }
}
