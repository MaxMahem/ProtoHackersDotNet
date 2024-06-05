using ProtoHackersDotNet.AsciiString;
using Vogen;

namespace ProtoHackersDotNet.Servers.BudgetChat;

[ValueObject<ascii>(throws: typeof(InvalidNameException))]
[SuppressMessage("Usage", "AddNormalizeInputMethod:Value Objects can have a method that normalizes (sanitizes) input", Justification = "Normalization not appropriate")]
public readonly partial record struct AsciiName
{
    public static Validation Validate(ascii name) => name.IsAlphanumeric() ? Validation.Ok 
        : Validation.Invalid("Name is not alphanumeric");
}

public class InvalidNameException(string message) : Exception(message)
{
    [DoesNotReturn]
    public static T Throw<T>(string message) => throw new InvalidNameException(message);
}