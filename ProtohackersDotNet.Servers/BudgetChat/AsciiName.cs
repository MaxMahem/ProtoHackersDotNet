using ProtoHackersDotNet.AsciiString;
using Vogen;

namespace ProtoHackersDotNet.Servers.BudgetChat;

[ValueObject<Ascii>]
readonly partial record struct AsciiName
{
    public static Validation Validate(Ascii name)
    {
        int MaxNameLength = 16;
        if (name.Length <= 0 || name.Length > MaxNameLength)
            return Validation.Invalid($"Name must be between [1-{MaxNameLength}], was ({name.Length}).");

        for (int index = 0; index < name.Length; index++) {
            if (!IsAlphanumeric(name[index]))
                return Validation.Invalid($"Name must be alphanumeric. Non-alphanumeric character `{(char) name[index]}` found at position ({index}).");
        }
        return Validation.Ok;
    }

    static bool IsAlphanumeric(byte c) 
        => c is (>= (byte) 'A' and <= (byte) 'Z')
             or (>= (byte) 'a' and <= (byte) 'z')
             or (>= (byte) '0' and <= (byte) '9');
}