using CommunityToolkit.Diagnostics;
using ProtoHackersDotNet.Servers.Helpers;
using ProtoHackersDotNet.Servers.PriceTracker;

namespace ProtoHackersDotNet.Servers.Helpers;

public static class SequenceHelper
{
    /// <summary>Finds the next position of <paramref name="value"/> in <paramref name="buffer"/> and returns the 
    /// postion offset by <paramref name="offset"/> elements or <see langword="null"/> if <paramref name="value"/>
    /// is abscent.</summary>
    /// <param name="buffer">The buffer to search.</param>
    /// <param name="value">The value to search for.</param>
    /// <param name="offset">The number of positions to offset the returned sequence.</param>
    /// <returns>The position of the first <paramref name="value"/> found offset by <paramref name="offset"/> 
    /// positions. Or <see langword="null"/> if abscent.</returns>
    public static SequencePosition? PositionOf<T>(this ReadOnlySequence<T> buffer, T value, int offset) where T : IEquatable<T>
        => buffer.PositionOf(value) is SequencePosition position ? buffer.GetPosition(offset, position) : null;

    /// <summary>Converts <paramref name="data"/> into a hex string divided lines <paramref name="bytesPerLine"/> long.</summary>
    public static string ToHexByteString(this ReadOnlySequence<byte> data, int bytesPerLine = 8)
    {
        Guard.IsLessThan(data.Length, int.MaxValue);
        int lineLength = (int)(data.Length / bytesPerLine * (Environment.NewLine.Length - 1) + data.Length * 3);
        return string.Create(lineLength, data, (chars, state) => {
            int bytesWrittenInLine = 0;
            int index = 0;

            foreach (ReadOnlyMemory<byte> segment in data) {
                foreach (byte b in segment.Span) {
                    chars[index++] = b.GetHighNibbleHex();
                    chars[index++] = b.GetLowNibbleHex();
                    chars[index++] = ' '; // Space between bytes
                    bytesWrittenInLine++;

                    if (bytesWrittenInLine % bytesPerLine == 0) {
                        index--;
                        Environment.NewLine.CopyTo(chars[index..]);
                        index += Environment.NewLine.Length;
                    }
                }
            }
        });
    }
}
