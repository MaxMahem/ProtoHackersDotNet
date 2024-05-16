namespace ProtoHackersDotNet.AsciiString;

public readonly partial struct Ascii
{
    /// <summary>Creates a new Ascii, seperated by <paramref name="delimiter"/> by joining the Asciis in 
    /// <paramref name="asciis"/>.</summary>
    /// <param name="delimiter">The delimiter to seperate <paramref name="asciis"/> by.</param>
    /// <param name="asciis">An enumeration of ascii to join.</param>
    /// <returns>A new Ascii, made up of <paramref name="asciis"/> seperated by <paramref name="delimiter"/>.</returns>
    public static Ascii Join(Ascii delimiter, IEnumerable<Ascii> asciis)
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
}