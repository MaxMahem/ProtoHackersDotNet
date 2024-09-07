using CommunityToolkit.HighPerformance;
using ProtoHackersDotNet.GUI.Serialization;

namespace ProtoHackersDotNet.GUI.MainView.EndPoint;

public class TextEndPoint : EndPointVM
{
    public TextEndPoint(SerializableEndPoint serializableEndPoint) : base(serializableEndPoint)
    {
        this.ipText = IP.Value?.ToString();
        IP.Changes.Subscribe(ip => IPText = ip?.ToString()).DiscardUnsubscribe();
    }

    /// <summary>Gets or sets the string representation of <see cref="EndPointVM.IP"/>.</summary>
    /// <remarks>Setting this value triggers a parse, and if succesfull, sets the <see cref="EndPointVM.IP"/> to the 
    /// parsed value. If unsucesfull, <see cref="EndPointVM.IP"/> is set to <c>null</c>.</remarks>
    public string? IPText
    {
        get => this.ipText;
        set
        {
            if (value == this.ipText) return;
            this.ipText = value;
            IP.Value = value switch
            {
                string value4 when TryParseIPv4(value4, out var ipv4) => ipv4,
                string value6 when value.Count(':') >= 2 && IPAddress.TryParse(value6, out var ipv6) => ipv6,
                _ => null,
            };
        }
    }
    string? ipText;

    /// <summary>Parses <paramref name="input"/> for a span in the xxx.xxx.xxx.xxx format.</summary>
    /// <remarks><see cref="IPAddress.TryParse(ReadOnlySpan{char}, out IPAddress?)"/> is to liberal in what it accepts
    /// as input, hence this method.</remarks>
    /// <param name="input">The ip address to try and parse.</param>
    /// <param name="ip">When this method returns <c>true</c> the parsed <see cref="IPAddress"/>, <c>null</c> otherwise.</param>
    /// <returns><c>true</c> if <paramref name="input"/> was parsed into an <see cref="IPAddress"/>,
    /// <c>false</c> otherwise.</returns>
    static bool TryParseIPv4(ReadOnlySpan<char> input, [NotNullWhen(true)] out IPAddress? ip)
    {
        ip = null;

        Span<byte> ipAddressValue = stackalloc byte[4];

        // step through the string by dotted segment, and the ip value by byte.
        var stringTokenizer = input.Tokenize('.');
        for (int byteIndex = 0; byteIndex < ipAddressValue.Length; byteIndex++)
        {
            if (!stringTokenizer.MoveNext()) return false; // too few segments.
            if (!byte.TryParse(stringTokenizer.Current, out ipAddressValue[byteIndex])) return false;
        }
        if (stringTokenizer.MoveNext()) return false; // too many segments

        ip = new IPAddress(ipAddressValue);
        return true;
    }
}