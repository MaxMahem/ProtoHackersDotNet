namespace ProtoHackersDotNet.Servers.PriceTracker;

public sealed partial class PriceTrackerClient(TcpClient client, CancellationToken token) : TcpClientBase(client, token)
{
    readonly ObservableValueList<PriceMessage> priceHistory = [];
    readonly byte[] response = new byte[sizeof(int)];

    public override IObservable<string?> Status => this.priceHistory.CurrentCount.Select(count => $"{count} entries");

    protected override SequencePosition? FindLineEnd(ReadOnlySequence<byte> buffer)
        => buffer.Length < PriceMessage.MESSAGE_LENGTH ? null : buffer.GetPosition(PriceMessage.MESSAGE_LENGTH);

    protected override async Task ProcessLine(ReadOnlySequence<byte> line, CancellationToken token)
    {
        var priceMessage = PriceMessage.Parse(line);

        switch (priceMessage) {
            case { Type: PriceMessageType.Insert } insertMessage:
                this.priceHistory.Add(insertMessage);
                break;
            case { Type: PriceMessageType.Query } queryMessage:
                int average = AveragePrice(queryMessage);

                BinaryPrimitives.WriteInt32BigEndian(response, average);
                QueryResponse message = new(this.response, queryMessage.MaxDateUtc, queryMessage.MaxDateUtc, average);

                await Transmit(message, token);
                break;
            default:
                ClientException.ReThrow(new InvalidDataException($"Invalid Message Format ({(byte) priceMessage.Type:X2})"), this);
                break;
        }
    }

    /// <summary>Averages the price of all data in price history, according to <paramref name="query"/>.</summary>
    /// <param name="query">The query to use to filter the price history.</param>
    /// <returns>The average integer price of the data in range, or 0 if there is no data, or if the query is ill formed.</returns>
    int AveragePrice(PriceMessage query) 
        => query.MinTime <= query.MaxTime ? (int) this.priceHistory.Where(query.IsInRange).Select(entry => entry.Price)
                                                                   .DefaultIfEmpty(0).Average() : 0;

    protected override string TranslateReception(ReadOnlySequence<byte> buffer)
    {
        var (quotient, remainder) = long.DivRem(buffer.Length, PriceMessage.MESSAGE_LENGTH);
        var messageWord = quotient is 1 ? "messages" : "message";
        var partialWord = remainder is not 0 ? "incomplete " : string.Empty;
        return $"{quotient} {messageWord} ({partialWord}{buffer.ToByteSize()})";
    }

    readonly struct QueryResponse(ReadOnlyMemory<byte> data, DateTimeOffset min, DateTimeOffset max, int average) 
        : ITransmission
    {
        public ReadOnlyMemory<byte> Data => data;
        public string Translation => $"Average price from {min:u} to {max:u}: {average/100.0:N2}";
    }
}