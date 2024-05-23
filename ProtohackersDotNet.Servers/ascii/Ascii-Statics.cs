using System.Text;
using Vogen;

namespace ProtoHackersDotNet.AsciiString;

#pragma warning disable CS8981
public readonly partial struct ascii
#pragma warning restore CS8981
{
    /// <summary>Creates a new Ascii, seperated by <paramref name="delimiter"/> by joining the Asciis in 
    /// <paramref name="asciis"/>.</summary>
    /// <param name="delimiter">The delimiter to seperate <paramref name="asciis"/> by.</param>
    /// <param name="asciis">An enumeration of ascii to join.</param>
    /// <returns>A new Ascii, made up of <paramref name="asciis"/> seperated by <paramref name="delimiter"/>.</returns>
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

    /// <summary>Returns the first index of any non-printable ascii characters (lower than 32, higher than 176) or -1
    /// if no non-ascii characters were found.</summary>
    /// <param name="value">The string to check.</param>
    /// <returns>The first index of a non-printable ascii character, or -1 if none are found.</returns>
    public static int IndexOfNonPrintableAscii(string value) => value.AsSpan().IndexOfAnyExceptInRange((char) 32, (char) 176);

    /// <summary>Returns the first index of any non-printable ascii characters (lower than 32, higher than 176) or -1
    /// if no non-ascii characters were found.</summary>
    /// <param name="value">The string to check.</param>
    /// <returns>The first index of a non-printable ascii character, or -1 if none are found.</returns>
    public static int IndexOfNonPrintableAscii(ReadOnlySpan<char> value) => value.IndexOfAnyExceptInRange((char) 32, (char) 176);
}

public static class StringToAsciiHelper
{
    public static bool IsValidAscii(this string value) => ascii.IndexOfNonPrintableAscii(value) <= 0;
    public static ascii ToAscii(this string value) => ascii.IndexOfNonPrintableAscii(value) is int index and >= 0
            ? ThrowHelper.ThrowArgumentException<ascii>($"Non ASCII character at index {index}.")
            : new(value);
}

public static class SpanToAsciiHelper
{
    public static bool IsValidAscii(this ReadOnlySpan<char> value) => ascii.IndexOfNonPrintableAscii(value) <= 0;
    public static ascii ToAscii(this ReadOnlySpan<char> value) => ascii.IndexOfNonPrintableAscii(value) is int index and >= 0
            ? ThrowHelper.ThrowArgumentException<ascii>($"Non ASCII character at index {index}.")
            : new(value);
}