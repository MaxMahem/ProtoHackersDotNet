namespace ProtoHackersDotNet.Servers.PriceTracker;

public sealed class PriceTrackerClient(TcpClient client, PriceTrackerServer service, CancellationToken token)
    : TcpClientBase<PriceTrackerServer, PriceTrackerClient>(client, service, token)
{
    readonly List<PriceMessage> priceHistory = [];
    readonly byte[] response = new byte[sizeof(int)];

    protected override SequencePosition? FindLineEnd(ReadOnlySequence<byte> buffer)
        => buffer.Length < PriceMessage.MESSAGE_LENGTH ? null : buffer.GetPosition(PriceMessage.MESSAGE_LENGTH);

    protected override async Task ProcessLine(ReadOnlySequence<byte> line)
    {
        var priceMessage = PriceMessage.Parse(line);
        // NotifyEvent(ClientEventType.LineTranslation, priceMessage.ToString());

        switch (priceMessage) {
            case { Type: PriceMessageType.Insert } insertMessage:
                priceHistory.Add(insertMessage);
                break;
            case { Type: PriceMessageType.Query } queryMessage:
                int average = AveragePrice(queryMessage);

                BinaryPrimitives.WriteInt32BigEndian(response, average);
                QueryResponse message = new(response, queryMessage.MaxDateUtc, queryMessage.MaxDateUtc, average);

                await Transmit(message);
                break;
            default: throw new FormatException($"Invalid Message Format ({priceMessage.Type}");
        }
    }

    /// <summary>Averages the price of all data in price history, according to <paramref name="queryMessage"/>.</summary>
    /// <param name="queryMessage">The query to use to filter the price history.</param>
    /// <returns>The average integer price of the data in range, or 0 if there is no data, or if the query is illformed.</returns>
    int AveragePrice(PriceMessage queryMessage) => queryMessage.MinTime <= queryMessage.MaxTime
            ? (int) this.priceHistory.Where(queryMessage.IsInRange).Select(historyMessage => historyMessage.Price)
                                      .DefaultIfEmpty(0).Average()
            : 0;

    protected override string TranslateReciept(ReadOnlySequence<byte> buffer) 
        => buffer.ToHexByteString(PriceMessage.MESSAGE_LENGTH);

    readonly struct QueryResponse(ReadOnlyMemory<byte> data, DateTimeOffset min, DateTimeOffset max, int average) : ITransmission
    {
        public ReadOnlyMemory<byte> Data => data;
        public string Translation => $"Average price from {min:u}-{max:u}: {average}.";
        public bool Broadcast => false;
    }
}
