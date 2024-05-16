using ProtoHackersDotNet.AsciiString;
using ProtoHackersDotNet.Servers.BudgetChat;

namespace ProtoHackersDotNet.Servers.BudgetChat;

static class AsciiMessageHelper
{
    public static AsciiTransmission ToTransmission(this Ascii ascii, bool broadcast) 
        => new(ascii, broadcast);
}
