﻿using System.Buffers;
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
        SequencePosition? lastNonMatchingPosition = null;
        SequencePosition newPosition = sequence.Start, oldPosition = sequence.Start;

        while (sequence.TryGet(ref newPosition, out var currentSegment))
        {
            if (currentSegment.Span.LastIndexOfAnyExcept(searchValues) is int foundIndex and >= 0)
                lastNonMatchingPosition = sequence.GetPosition(foundIndex, oldPosition);
            oldPosition = newPosition;
        }

        return lastNonMatchingPosition;
    }

    /// <summary>Searches for the first position of any value outside of the range between <paramref name="lowInclusive"/> and 
    /// <paramref name="highInclusive"/>, inclusive, or <see langword="null"/> if no value was found.</summary>
    /// <typeparam name="T">The type of the sequence and values.</typeparam>
    /// <param name="span">The sequence to search.</param>
    /// <param name="lowInclusive">A lower bound, inclusive, of the excluded range.</param>
    /// <param name="highInclusive">A upper bound, inclusive, of the excluded range.</param>
    /// <returns>The position of the first occurrence of any value outside of the specified range.
    /// If all of the values are inside of the specified range, returns <see langword="null"/>.</returns>
    public static SequencePosition? PositionOfAnyExceptInRange<T>(this ReadOnlySequence<T> sequence, T lowInclusive, T highInclusive)
        where T : unmanaged, IEquatable<T>, IComparable<T>
    {
        SequencePosition newPosition = sequence.Start, oldPosition = sequence.Start;

        while (sequence.TryGet(ref newPosition, out var currentSegment)) {
            int foundIndex = currentSegment.Span.IndexOfAnyExceptInRange(lowInclusive, highInclusive);

            if (foundIndex is not -1) return sequence.GetPosition(foundIndex, oldPosition);
            oldPosition = newPosition;
        };

        return null;
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

    /// <summary>Trims <paramref name="count"/> number of elements from the end of <paramref name="sequence"/>.</summary>
    /// <typeparam name="T">The type of the sequence.</typeparam>
    /// <param name="sequence">The sequence to trim.</param>
    /// <param name="count">The number of elements to trim.</param>
    /// <returns>The sequence, trimmed of <paramref name="count"/> elements from the end.</returns>
    public static ReadOnlySequence<T> TrimEnd<T>(this ReadOnlySequence<T> sequence, int count) 
        => sequence.Slice(0, sequence.Length - count);

    /// <summary>Split <paramref name="input"/> into two parts at <paramref name="position"/>.</summary>
    /// <param name="input">The sequence to be split.</param>
    /// <param name="position">The position at which to split the sequence.</param>
    /// <returns>A tuple containing two <see cref="ReadOnlySequence{T}"/> instances:
    /// <list type="bullet">
    /// <item><description><c>First</c>: The segment from the start up to, but not including, the specified position.</description></item>
    /// <item><description><c>Second</c>: The segment from the specified position to the end.</description></item>
    /// </list></returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="position"/> is invalid for <paramref name="input"/>.</exception>
    public static (ReadOnlySequence<T>, ReadOnlySequence<T>) Split<T>(this ReadOnlySequence<T> input, SequencePosition position) 
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