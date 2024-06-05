using ProtoHackersDotNet.AsciiString;

namespace ProtoHackersDotNet.Servers.BudgetChat;

public class AsciiTransmission(ascii ascii)
    : ITransmission
{
    public ReadOnlyMemory<byte> Data => ascii.AsMemory();
    public string Translation => ascii.ToString();
}