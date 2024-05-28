using ProtoHackersDotNet.AsciiString;
using ProtoHackersDotNet.Servers.Interface.Client.Messages;

namespace ProtoHackersDotNet.Servers.BudgetChat;

public class AsciiTransmission(ascii ascii, bool broadcast)
    : ITransmission
{
    public AsciiTransmission(string message, bool broadcast)
        : this(new ascii(message), broadcast) { }
    public ReadOnlyMemory<byte> Data => ascii.AsMemory();
    public string Translation => ascii.ToString();
    public ByteSize BytesTransmitted => ByteSize.FromBytes(Data.Length);
    public bool Broadcast => broadcast;
}