using CommunityToolkit.HighPerformance;

namespace ProtoHackersDotNet.GUI.MainView.EndPoint;

public partial class TextEndPoint(IPAddress? ip, ushort? port) : EndPointVM(ip, port)
{
    string? _IPText = ip?.ToString();
    public string? IPText
    {
        get => _IPText;
        set
        {
            if (value == _IPText) return;
            _IPText = value;
            IP = value switch
            {
                string value4 when value.Count('.') is 3 && TryParseIPv4(value4, out var ipv4) => ipv4,
                string value6 when value.Contains(':') && IPAddress.TryParse(value6, out var ipv6) => ipv6,
                _ => null,
            };
        }
    }

    static bool TryParseIPv4(ReadOnlySpan<char> input, [NotNullWhen(true)] out IPAddress? ip)
    {
        ip = null;

        Span<byte> ipAddressValue = stackalloc byte[4];

        // step through the string by dotted segment, and the ip by bytes.
        var stringTokenizer = input.Tokenize('.');
        for (int byteIndex = 0; byteIndex < ipAddressValue.Length; byteIndex++)
        {
            if (!stringTokenizer.MoveNext()) return false; // too few segments.
            if (!byte.TryParse(stringTokenizer.Current, out ipAddressValue[byteIndex])) return false;
        }
        if (stringTokenizer.MoveNext()) return false; // reject any trailing segments

        ip = new IPAddress(ipAddressValue);
        return true;
    }
}