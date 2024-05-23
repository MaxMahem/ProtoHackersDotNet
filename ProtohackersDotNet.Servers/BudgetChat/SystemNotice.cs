using Vogen;
using System.Text.RegularExpressions;

namespace ProtoHackersDotNet.Servers.BudgetChat;

[ValueObject<PrefixPostfixAscii>]
[SuppressMessage("Usage", "AddValidationMethod:Value Objects can have validation", Justification = "From includes validation.")]
public readonly partial struct SystemNotice
{
    public static SystemNotice From(string notice)
    {
        var prefixPostfix = PrefixPostfixAscii.From(notice, SystemMessageRegex());
        return From(prefixPostfix);
    }

    [GeneratedRegex(@"(^\*[ -~]+){n}([ -~]*$)")]
    private static partial Regex SystemMessageRegex();
}
