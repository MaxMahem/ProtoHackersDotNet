using ProtoHackersDotNet.AsciiString;
using System.Text.RegularExpressions;

namespace ProtoHackersDotNet.Servers.BudgetChat;

public readonly partial record struct PrefixPostfixAscii(ascii Prefix, ascii Postfix)
{
    public static PrefixPostfixAscii From(string input, Regex pattern)
    {
        (Group Prefix, Group Postfix) matches = pattern.Matches(input) is [{ Groups: [_, var group1, var group2]}] ? (group1, group2) 
            : ThrowHelper.ThrowFormatException<(Group, Group)>();
        return new(matches.Prefix.ValueSpan.ToAscii(), matches.Postfix.ValueSpan.ToAscii());
    }
}