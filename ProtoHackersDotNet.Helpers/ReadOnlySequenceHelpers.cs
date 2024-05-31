using System.Buffers;
using CommunityToolkit.Diagnostics;

namespace ProtoHackersDotNet.Helpers;

public static class ReadOnlySequenceHelpers
{
    /// <summary>Finds the position of any value other than <paramref name="searchValues"/>. 
    /// Returns null if no value is found.</summary>
    /// <typeparam name="T">The type of values to operate on.</typeparam>
    /// <param name="sequence">The sequence to search.</param>
    /// <param name="searchValues">The values to avoid.</param>
    /// <returns>The position of the first occurrence of any <typeparamref name="T"/> except those in <paramref name="searchValues"/>.
    /// or <c>null</c> if all of the <typeparamref name="T"/> are in <paramref name="searchValues"/>.</returns>
    public static SequencePosition? LastPositionOfAnyExcept<T>(this ReadOnlySequence<T> sequence, ReadOnlySpan<T> searchValues)
        where T : unmanaged, IEquatable<T>
    {
        if (sequence.IsEmpty) return null;

        var reader = new SequenceReader<T>(sequence);
        SequencePosition? lastNonMatchingPosition = null;
        long inspectedLength = 0;

        while (!reader.End)
        {
            ReadOnlySpan<T> currentSpan = reader.UnreadSpan;
            int foundIndex = currentSpan.LastIndexOfAnyExcept(searchValues);

            if (foundIndex is not -1)
                lastNonMatchingPosition = reader.Sequence.GetPosition(inspectedLength + foundIndex, sequence.Start);

            inspectedLength += currentSpan.Length;
            reader.Advance(currentSpan.Length);
        }

        return lastNonMatchingPosition;
    }

    /// <summary>Finds the next position of <paramref name="value"/> in <paramref name="buffer"/> and returns the 
    /// position offset by <paramref name="offset"/> elements or <see langword="null"/> if <paramref name="value"/>
    /// is absent.</summary>
    /// <remarks>Since <see cref="ReadOnlySequence{T}.Slice(SequencePosition, SequencePosition)"/> is [Inclusive, Exclusive)
    /// it is often useful to get the position just beyond a marker so a slice can be done inclusive of a marker.</remarks>
    /// <param name="buffer">The buffer to search.</param>
    /// <param name="value">The value to search for.</param>
    /// <param name="offset">The number of positions to offset the returned sequence.</param>
    /// <returns>The position of the first <paramref name="value"/> found offset by <paramref name="offset"/>
    /// positions. Or <see langword="null"/> if absent.</returns>
    public static SequencePosition? PositionOf<T>(this ReadOnlySequence<T> buffer, T value, int offset) where T : IEquatable<T>
        => buffer.PositionOf(value) is SequencePosition position ? buffer.GetPosition(offset, position) : null;

    /// <summary>Divides <paramref name="input"/> into two parts at <paramref name="position"/>.</summary>
    /// <param name="input">The sequence to be split.</param>
    /// <param name="position">The position at which to split the sequence.</param>
    /// <returns>A tuple containing two <see cref="ReadOnlySequence{T}"/> instances:
    /// <list type="bullet">
    /// <item><description><c>First</c>: The segment from the start up to, but not including, the specified position.</description></item>
    /// <item><description><c>Second</c>: The segment from the specified position to the end.</description></item>
    /// </list></returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="position"/> is invalid for <paramref name="input"/>.</exception>
    public static (ReadOnlySequence<T>, ReadOnlySequence<T>) Divide<T>(this ReadOnlySequence<T> input, SequencePosition position) 
        => (input.Slice(0, position), input.Slice(position));

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