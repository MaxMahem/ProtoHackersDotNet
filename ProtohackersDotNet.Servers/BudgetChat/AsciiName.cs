using ProtoHackersDotNet.AsciiString;
using Vogen;

namespace ProtoHackersDotNet.Servers.BudgetChat;

[ValueObject<ascii>]
public readonly partial record struct AsciiName
{
    public static Validation Validate(ascii name) => name.IsAlphanumeric() ? Validation.Ok 
        : Validation.Invalid("Name is not alphanumeric");
}