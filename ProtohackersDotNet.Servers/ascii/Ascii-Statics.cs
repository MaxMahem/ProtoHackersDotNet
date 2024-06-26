﻿namespace ProtoHackersDotNet.AsciiString;

#pragma warning disable CS8981
[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Fundamental Type")]
public readonly partial struct ascii
#pragma warning restore CS8981
{
    /// <summary>Creates a new Ascii, separated by <paramref name="delimiter"/> by joining the Asciis in 
    /// <paramref name="asciis"/>.</summary>
    /// <param name="delimiter">The delimiter to separate <paramref name="asciis"/> by.</param>
    /// <param name="asciis">An enumeration of ascii to join.</param>
    /// <returns>A new Ascii, made up of <paramref name="asciis"/> separated by <paramref name="delimiter"/>.</returns>
    public static ascii Join(ascii delimiter, IEnumerable<ascii> asciis)
    {
        using var enumerator = asciis.GetEnumerator();
        if (!enumerator.MoveNext())
            return Empty;

        ValueAsciiBuilder builder = new(stackalloc byte[1024]);
        builder.Append(enumerator.Current);

        while (enumerator.MoveNext()) {
            builder.Append(delimiter);
            builder.Append(enumerator.Current);
        }

        return builder.ToAscii();
    }

    /// <summary>Returns the first index of any non ascii characters (0, higher than 126) or -1
    /// if no non-ascii characters were found.</summary>
    /// <param name="span">The string to check.</param>
    /// <returns>The first index of a non-printable ascii character, or -1 if none are found.</returns>
    public static int IndexOfNonAscii(ReadOnlySpan<char> span) => span.IndexOfAnyExceptInRange((char) 0, (char) 127);

    /// <summary>Returns the first index of any non ascii characters (0, higher than 126) or -1
    /// if no non-ascii characters were found.</summary>
    /// <param name="span">The string to check.</param>
    /// <returns>The first index of a non-printable ascii character, or -1 if none are found.</returns>
    public static int IndexOfNonAscii(ReadOnlySpan<byte> span) => span.IndexOfAnyExceptInRange((byte) 0, (byte) 127);

    public static SequencePosition? PositionOfNonAscii(ReadOnlySequence<byte> sequence)
    {
        if (sequence.IsEmpty) return null;

        var reader = new SequenceReader<byte>(sequence);
        long inspectedLength = 0;

        while (!reader.End) {
            ReadOnlySpan<byte> currentSpan = reader.UnreadSpan;
            int foundIndex = currentSpan.IndexOfAnyInRange((byte) 128, byte.MaxValue);

            if (foundIndex is not -1) return reader.Sequence.GetPosition(inspectedLength + foundIndex, sequence.Start);

            inspectedLength += currentSpan.Length;
            reader.Advance(currentSpan.Length);
        }

        return null;
    }

    /// <summary>Returns the first index of any non-printable ascii characters (lower than 32, higher than 126) or -1
    /// if no non-ascii characters were found.</summary>
    /// <param name="span">The string to check.</param>
    /// <returns>The first index of a non-printable ascii character, or -1 if none are found.</returns>
    public static int IndexOfNonPrintableAscii(ReadOnlySpan<char> span) 
        => span.IndexOfAnyExceptInRange((char) 32, (char) 126);
}

public static class StringToAsciiHelper
{
    public static bool IsValidAscii(this string value) => ascii.IndexOfNonAscii(value) <= 0;
    public static ascii ToAscii(this string value) => ascii.IndexOfNonAscii(value) is int index and >= 0
            ? ThrowArgumentException<ascii>($"Non ASCII character at index {index}")
            : new(value);
}

public static class SpanToAsciiHelper
{
    public static bool IsValidAscii(this ReadOnlySpan<char> span) => ascii.IndexOfNonAscii(span) <= 0;

    public static ascii ToAscii(this ReadOnlySpan<char> span) => ascii.IndexOfNonAscii(span) is int index and >= 0
            ? ThrowArgumentException<ascii>($"Non ASCII character at index {index}")
            : new(span);

    public static bool IsValidAscii(this ReadOnlySpan<byte> span) => ascii.IndexOfNonAscii(span) <= 0;

    public static ascii ToAscii(this ReadOnlySpan<byte> span) => ascii.IndexOfNonAscii(span) is int index and >= 0
            ? ThrowArgumentException<ascii>($"Non ASCII character at index {index}")
            : new(span);
}

public static class SequenceAsciiHelper
{
    public static bool IsValidAscii(this ReadOnlySequence<byte> value) => ascii.PositionOfNonAscii(value) is not null;
    public static ascii ToAscii(this ReadOnlySequence<byte> value) => ascii.PositionOfNonAscii(value) is not null
            ? ThrowArgumentException<ascii>($"Non ASCII character found")
            : new(value);

    /// <summary>Determines if all values in this <paramref name="sequence"/> are Alphanumeric (0-9, A-Z, a-z).</summary>
    /// <param name="sequence">The sequence to inspect.</param>
    /// <returns><see langword="true"/> if all values in the sequence are alphanumeric, <see langword="false"/> otherwise.</returns>
    public static bool IsAlphanumeric(this ReadOnlySequence<byte> sequence)
        => sequence.PositionOfAnyExceptInRange<byte>(0x00, 0x2F) is not null  // Control characters and punctuation
		&& sequence.PositionOfAnyExceptInRange<byte>(0x3A, 0x40) is not null  // : ; < = > ? @
        && sequence.PositionOfAnyExceptInRange<byte>(0x5B, 0x60) is not null  // [ \ ] ^ _ `
        && sequence.PositionOfAnyExceptInRange<byte>(0x7B, 0xFF) is not null; // { | } ~ and beyond
}