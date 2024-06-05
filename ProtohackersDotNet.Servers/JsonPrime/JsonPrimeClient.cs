namespace ProtoHackersDotNet.Servers.JsonPrime;

public sealed class JsonPrimeClient(TcpClient client, CancellationToken token) : TcpClientBase(client, token)
{
    public const byte LINE_DELIMITER = (byte) '\n';

    protected override SequencePosition? FindLineEnd(ReadOnlySequence<byte> buffer)
        => buffer.PositionOf(LINE_DELIMITER, 1);

    protected override async Task ProcessLine(ReadOnlySequence<byte> line, CancellationToken token)
    {
        try {
            var response = ProcessLineQuery(line).Number.IsPrime() ? IsPrime
                                                                   : NotPrime;
            await Transmit(response, token);
        }
        catch (Exception exception) {
            ClientException.ReThrow(exception, this);
        }
    }

protected override string TranslateReception(ReadOnlySequence<byte> buffer)
        => Encoding.UTF8.GetString(buffer);

    protected override async Task OnException(Exception exception, CancellationToken token)
        => await Transmit(Malformed);

    static PrimeQuery ProcessLineQuery(ReadOnlySequence<byte> line)
    {
        var jsonReader = new Utf8JsonReader(line);
        return JsonSerializer.Deserialize(ref jsonReader, JsonPrimeMetaData.Default.PrimeQuery).Validate()
            ?? ThrowJsonException<PrimeQuery>("Invalid Query");
    }

    readonly static CachedUtf8Response Malformed = new("malformed" + (char) LINE_DELIMITER);
    readonly static CachedUtf8Response IsPrime
        = new(JsonSerializer.Serialize(PrimeResponse.True,  JsonPrimeMetaData.Default.PrimeResponse) + (char) LINE_DELIMITER);
    readonly static CachedUtf8Response NotPrime
        = new(JsonSerializer.Serialize(PrimeResponse.False, JsonPrimeMetaData.Default.PrimeResponse) + (char) LINE_DELIMITER);

    [DoesNotReturn]
    static T ThrowJsonException<T>(string message) => throw new JsonException(message);
}