namespace ProtoHackersDotNet.Servers.PriceTracker;

public sealed partial class PriceTrackerClient
{
    readonly record struct PriceMessage
    {
        public const int MESSAGE_LENGTH = 9;

        public readonly PriceMessageType Type { get; init; }
        public readonly int Int1 { get; init; }
        public readonly int Int2 { get; init; }

        static PriceMessage Parse(ReadOnlySpan<byte> line) => new() {
            Type = (PriceMessageType) line[0],
            Int1 = BinaryPrimitives.ReadInt32BigEndian(line[1..5]),
            Int2 = BinaryPrimitives.ReadInt32BigEndian(line[5..9]),
        };

        public static PriceMessage Parse(ReadOnlySequence<byte> line)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(line.Length, MESSAGE_LENGTH);

            if (line.IsSingleSegment)
                return Parse(line.FirstSpan);
            else {
                Span<byte> lineSpan = stackalloc byte[MESSAGE_LENGTH];
                line.CopyTo(lineSpan);
                return Parse(lineSpan);
            }
        }

        public override string ToString() => Type switch {
            PriceMessageType.Query => $"Query {{ MinTime: {MinDateUtc:u}, MaxTime: {MaxDateUtc:u} }}",
            PriceMessageType.Insert => $"Insert {{ Timestamp: {TimestampUtc:u}, Price: {Price} }}",
            _ => ThrowHelper.ThrowInvalidOperationException<string>($"Invalid Type ({Type})")
        };

        public int Timestamp => Int1;
        public int Price => Int2;
        public DateTimeOffset TimestampUtc => DateTimeOffset.FromUnixTimeSeconds(Timestamp);

        public int MinTime => Int1;
        public int MaxTime => Int2;
        public DateTimeOffset MinDateUtc => DateTimeOffset.FromUnixTimeSeconds(MinTime);
        public DateTimeOffset MaxDateUtc => DateTimeOffset.FromUnixTimeSeconds(MaxTime);

        public bool IsInRange(PriceMessage insertMessage)
            => insertMessage.Timestamp >= MinTime && insertMessage.Timestamp <= MaxTime;
    }
}