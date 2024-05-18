namespace ProtoHackersDotNet.Servers.PriceTracker;

public sealed partial class PriceTrackerClient(TcpClient client, PriceTrackerServer service, CancellationToken token)
    : TcpClientBase<PriceTrackerServer, PriceTrackerClient>(client, service, token), IClient
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
                this.priceHistory.Add(insertMessage);
                Status = $"{this.priceHistory.Count} entries";
                break;
            case { Type: PriceMessageType.Query } queryMessage:
                int average = AveragePrice(queryMessage);

                BinaryPrimitives.WriteInt32BigEndian(response, average);
                QueryResponse message = new(this.response, queryMessage.MaxDateUtc, queryMessage.MaxDateUtc, average);

                await Transmit(message);
                break;
            default: throw new FormatException($"Invalid Message Format ({priceMessage.Type:X})");
        }
    }

    /// <summary>Averages the price of all data in price history, according to <paramref name="query"/>.</summary>
    /// <param name="query">The query to use to filter the price history.</param>
    /// <returns>The average integer price of the data in range, or 0 if there is no data, or if the query is illformed.</returns>
    int AveragePrice(PriceMessage query) 
        => query.MinTime <= query.MaxTime ? (int) this.priceHistory.Where(query.IsInRange).Select(entry => entry.Price)
                                                                   .DefaultIfEmpty(0).Average() : 0;

    protected override string TranslateReciept(ReadOnlySequence<byte> buffer)
        => $"{buffer.Length / (double) PriceMessage.MESSAGE_LENGTH:0.#} messages ({buffer.ToByteSize()})";

    readonly struct QueryResponse(ReadOnlyMemory<byte> data, DateTimeOffset min, DateTimeOffset max, int average) : ITransmission
    {
        public ReadOnlyMemory<byte> Data => data;
        public string Translation => $"Average price from {min:u} to {max:u}: {average}.";
        public bool Broadcast => false;
    }
}
