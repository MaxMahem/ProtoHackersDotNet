using ProtoHackersDotNet.Servers.Helpers;
using ProtoHackersDotNet.Servers.TcpServer;
using ProtoHackersDotNet.Servers.JsonPrime;
using System.Text;
using System.Text.Json;

namespace ProtoHackersDotNet.Servers.JsonPrime;

public sealed class JsonPrimeClient(TcpClient client, JsonPrimeServer service, CancellationToken token)
    : TcpClientBase<JsonPrimeServer, JsonPrimeClient>(client, service, token)
{
    public const byte LINE_DELIMITER = (byte) '\n';

    protected override SequencePosition? FindLineEnd(ReadOnlySequence<byte> buffer)
        => buffer.PositionOf(LINE_DELIMITER, 1);

    protected override async Task ProcessLine(ReadOnlySequence<byte> line)
    {
        var query = ProcessLineQuery(line);
        // NotifyEvent(ClientEventType.LineTranslation, query.ToString());

        var response = query.Number.IsPrime() ? JsonPrimeSourceGenerationContext.IsPrime
                                              : JsonPrimeSourceGenerationContext.NotPrime;
        await Transmit(response);
    }

    protected override string TranslateReciept(ReadOnlySequence<byte> buffer)
        => Encoding.UTF8.GetString(buffer);

    protected override async Task OnException(Exception exception)
        => await Transmit(JsonPrimeSourceGenerationContext.Malformed);

    static PrimeQuery ProcessLineQuery(ReadOnlySequence<byte> line)
    {
        var jsonReader = new Utf8JsonReader(line);
        return JsonSerializer.Deserialize<PrimeQuery>(ref jsonReader, JsonPrimeSourceGenerationContext.JsonPrimeOptions).Validate();
    }
}

