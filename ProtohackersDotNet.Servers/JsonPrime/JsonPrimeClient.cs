using System.Text;
using System.Text.Json;

namespace ProtoHackersDotNet.Servers.JsonPrime;

public sealed class JsonPrimeClient(JsonPrimeServer server, TcpClient client, CancellationToken token)
    : TcpClientBase<JsonPrimeServer>(server, client, token)
{
    public const byte LINE_DELIMITER = (byte) '\n';

    protected override SequencePosition? FindLineEnd(ReadOnlySequence<byte> buffer)
        => buffer.PositionOf(LINE_DELIMITER, 1);

    protected override async Task ProcessLine(ReadOnlySequence<byte> line)
    {
        var query = ProcessLineQuery(line);
        // NotifyEvent(ClientEventType.LineTranslation, query.ToString());

        var response = query.Number.IsPrime() ? IsPrime
                                              : NotPrime;
        await Transmit(response);
    }

    protected override string TranslateRecieption(ReadOnlySequence<byte> buffer)
        => Encoding.UTF8.GetString(buffer);

    protected override async Task OnException(Exception exception)
        => await Transmit(Malformed);

    static PrimeQuery ProcessLineQuery(ReadOnlySequence<byte> line)
    {
        var jsonReader = new Utf8JsonReader(line);
        return JsonSerializer.Deserialize(ref jsonReader, JsonPrimeMetaData.Default.PrimeQuery).Validate();
    }

    readonly static CachedUtf8Response Malformed = new("malformed" + (char) LINE_DELIMITER);
    readonly static CachedUtf8Response IsPrime
        = new(JsonSerializer.Serialize(PrimeResponse.True,  JsonPrimeMetaData.Default.PrimeResponse) + (char) LINE_DELIMITER);
    readonly static CachedUtf8Response NotPrime
        = new(JsonSerializer.Serialize(PrimeResponse.False, JsonPrimeMetaData.Default.PrimeResponse) + (char) LINE_DELIMITER);
}