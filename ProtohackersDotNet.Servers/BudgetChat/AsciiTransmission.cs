using ProtoHackersDotNet.AsciiString;

namespace ProtoHackersDotNet.Servers.BudgetChat;

public class AsciiTransmission(Ascii ascii, bool broadcast)
    : ITransmission
{
    public AsciiTransmission(string message, bool broadcast)
        : this(new Ascii(message), broadcast) { }
    public ReadOnlyMemory<byte> Data => ascii.AsMemory();
    public string Translation => ascii.ToString();
    public ByteSize BytesTransmitted => ByteSize.FromBytes(Data.Length);
    public bool Broadcast => broadcast;
}