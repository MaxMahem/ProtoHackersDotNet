namespace ProtoHackersDotNet.Helpers;

public static class ByteHelper
{
    public static char GetHighNibbleHex(this byte b)
    {
        b >>= 4;
        return (char) (b < 10 ? b + '0' : b + 'A' - 10);
    }

    public static char GetLowNibbleHex(this byte b)
    {
        b &= 0x0F;
        return (char) (b < 10 ? b + '0' : b + 'A' - 10);
    }
}