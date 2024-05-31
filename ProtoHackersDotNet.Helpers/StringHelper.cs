using System.Text;

namespace ProtoHackersDotNet.Servers.Helpers;

public static class StringHelper
{
    public static byte[] ToBytes(this string input, Encoding encoding) => encoding.GetBytes(input);
}
