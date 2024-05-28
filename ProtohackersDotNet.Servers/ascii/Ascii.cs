using System.IO.Hashing;

namespace ProtoHackersDotNet.AsciiString;

#pragma warning disable CS8981 // lower case names may be reserved. If C# makes an ascii type that would be great.
public readonly partial struct ascii : IEquatable<ascii>, IComparable<ascii>
#pragma warning restore CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
{
    #region Data

    /// <summary>The backing field for this instance. References to this value should never be shared with outside code.</summary>
    readonly byte[]? chars;
    
    /// <summary>The length of the Ascii string.</summary>
    /// chars is intentionally not null checked here to let the JIT inline the bounds check.
    public readonly int Length => this.chars!.Length;

    #endregion

    #region Internal Methods

    /// <summary>Throws a nre if not initialized.</summary>
    // Very fast way of forcing a throw if not initialized, since Length will always need to be touched anyways.
    void ThrowNullRefIfNotInitialized() => _ = this.chars!.Length;

    /// <summary>Creates an Ascii *without* making a defensive copy.</summary>
    ascii(byte[]? array) => this.chars = array;

    #endregion

    #region Constructors

    public ascii(ReadOnlySpan<byte> span)
    {
        if (IndexOfNonAscii(span) is int index and >= 0) ThrowHelper.ThrowArgumentException($"Non ASCII character at index {index}");
        this.chars = span.ToArray();
    }

    public ascii(string str)
    {
        if (IndexOfNonAscii(str) is int index and >= 0) ThrowHelper.ThrowArgumentException($"Non ASCII character at index {index}");
        this.chars = [.. Encoding.ASCII.GetBytes(str)];
    }

    public ascii(ReadOnlySpan<char> span)
    {
        if (IndexOfNonAscii(span) is int index and >= 0) ThrowHelper.ThrowArgumentException($"Non ASCII character at index {index}");
        this.chars = new byte[span.Length];
        _ = Encoding.ASCII.GetBytes(span, this.chars) == span.Length || ThrowHelper.ThrowInvalidOperationException<bool>();
    }

    public ascii(ReadOnlySequence<byte> sequence) => this.chars = sequence.ToArray();

    #endregion

    [SuppressMessage("Performance", "CA1825:Avoid zero-length array allocations", Justification = "Designed copied from ImmutableArray")]
    public static readonly ascii Empty = new(new byte[0]);

    #region Equality and Hash

    /// <summary>Returns a hash code for this Ascii.</summary>
    /// <returns>A hash code for this instance, equal values should hash equally.</returns>
    public override int GetHashCode()
    {
        ascii self = this;
        return self.chars is null ? 0 : unchecked((int) XxHash32.HashToUInt32(self.chars));
    }

    /// <summary>Determines whether the specified <see cref="object"/> is equal to this Ascii.</summary>
    /// <param name="obj">The <see cref="object"/> to compare with this Ascii.</param>
    /// <returns><c>true</c> if the specified <see cref="object"/> is equal to this instance; otherwise, <c>false</c>.</returns>
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is ascii other && other == this;

    /// <summary>Indicates whether the current Ascii is equal to another Ascii.</summary>
    /// <param name="other">An Ascii to compare with.</param>
    /// <returns><c>true</c> if the current Ascii has an equal value to <paramref name="other"/> parameter; otherwise, <c>false</c>.
    /// </returns>
    public bool Equals(ascii other)
    {
        ascii self = this;
        
        if (ReferenceEquals(self.chars, other.chars)) return true;
        if (self.Length != other.Length) return false;
        return self.chars.AsSpan().SequenceEqual(other.chars);
    }

    public bool IsEmpty() => this.Length <= 0;

    public bool IsAlphanumeric()
    {
        var self = this;
        self.ThrowNullRefIfNotInitialized();

        for (int index = 0; index < self.Length; index++)
            if (!IsAlphanumeric(self[index]))
                return false;
        return true;
    }

    static bool IsAlphanumeric(byte c)
        => c is (>= (byte) 'A' and <= (byte) 'Z')
             or (>= (byte) 'a' and <= (byte) 'z')
             or (>= (byte) '0' and <= (byte) '9');

    /// <inheritdoc/>
    /// TODO: ENSURE THIS WORKS!
    public int CompareTo(ascii other)
    {
        ascii self = this;

        if (ReferenceEquals(self.chars, other.chars)) return 0;

        // They can't both be null at this point.
        if (self.chars  is null) return -1;
        if (other.chars is null) return 1;

        // TODO: Investigate some way to vectorize this check.
        int minLength = int.Min(self.Length, other.Length);
        for (int index = 0; index < minLength; index++)
        {
            if (self.chars[index] < other.chars[index]) return -1;
            if (self.chars[index] > other.chars[index]) return 1;
        }

        if (self.Length < other.Length) return -1;
        if (self.Length > other.Length) return 1;
        return 0;
    }

    #endregion

    #region Operators

    /// <summary>Checks value equality between two Ascii.</summary>
    /// <param name="left">The Ascii to the left of the operator.</param>
    /// <param name="right">The Ascii to the right of the operator.</param>
    /// <returns><c>true</c> if the two ascii have identical values; <c>false</c> otherwise.</returns>
    public static bool operator ==(ascii left, ascii right) => left.chars.AsSpan().SequenceEqual(right.chars);

    /// <summary>Checks value inequality between two Ascii.</summary>
    /// <param name="left">The Ascii to the left of the operator.</param>
    /// <param name="right">The Ascii to the right of the operator.</param>
    /// <returns><c>true</c> if the two ascii have different values; <c>false</c> otherwise.</returns>
    public static bool operator !=(ascii left, ascii right) => !left.chars.AsSpan().SequenceEqual(right.chars);

    #endregion

    /// <summary>Gets the character at the specified index</summary>
    /// <param name="index">The zero-based index of the character to get.</param>
    /// <returns>The character at the specified index.</returns>
    /// This is not null checked to allow the JIT to inline the bounds check. It must be trivial.
    public byte this[int index] => this.chars![index];

    /// <summary>Gets a value indicating whether this struct was initialized without an actual array instance.</summary>
    public bool IsDefault => this.chars is null;

    
    #region CopyTo

    /// <summary>Copies the contents of this Ascii to the specified array.</summary>
    /// <param name="destination">The array to copy to.</param>
    public readonly void CopyTo(byte[] destination)
    {
        ascii self = this;
        self.ThrowNullRefIfNotInitialized();
        Array.Copy(self.chars!, destination, self.Length);
    }

    /// <summary>Copies the contents of this Ascii to the specified array.</summary>
    /// <param name="destination">The array to copy to.</param>
    /// <param name="destinationIndex">The index into the destination array to which the first copied char is written.</param>
    public void CopyTo(byte[] destination, int destinationIndex)
    {
        ascii self = this;
        self.ThrowNullRefIfNotInitialized();
        Array.Copy(self.chars!, 0, destination, destinationIndex, self.Length);
    }

    /// <summary>Copies the contents of this Ascii to <paramref name="destination"/>.</summary>
    /// <param name="sourceIndex">The index into this collection of the first element to copy.</param>
    /// <param name="destination">The array to copy to.</param>
    /// <param name="destinationIndex">The index into the destination array to which the first copied char is written.</param>
    /// <param name="length">The number of chars to copy.</param>
    public void CopyTo(int sourceIndex, byte[] destination, int destinationIndex, int length)
    {
        ascii self = this;
        self.ThrowNullRefIfNotInitialized();
        Array.Copy(self.chars!, sourceIndex, destination, destinationIndex, length);
    }

    /// <summary>Copies the contents of this Ascii to <paramref name="destination"/>.</summary>
    /// <param name="destination">The <see cref="Span{byte}"/> that is the destination of the characters copied.</param>
    public void CopyTo(Span<byte> destination)
    {
        ascii self = this;
        self.ThrowNullRefIfNotInitialized();
        Guard.HasSizeGreaterThan(destination, self.Length);
        self.chars.CopyTo(destination);
    }

    #endregion

    public bool Contains(ascii needle) => chars.AsSpan().IndexOf(needle.AsSpan()) >= 0;

    public static ascii operator +(ascii a, ascii b)
    {
        int length = a.Length + b.Length;
        var buffer = length <= 1024 ? stackalloc byte[length] : new byte[length];

        a.chars.CopyTo(buffer);
        b.chars.CopyTo(buffer[a.Length..]);
        return new(buffer);
    }

    public static ascii operator+(ascii a, byte b)
    {
        int length = a.Length + 1;
        var buffer = length <= 1024 ? stackalloc byte[length] : new byte[length];

        a.chars.CopyTo(buffer);
        buffer[^1] = b;
        return new(buffer);
    }

    public ReadOnlySpan<byte> AsSpan() => this.chars.AsSpan();
    public ReadOnlyMemory<byte> AsMemory() => this.chars.AsMemory();

    public override string ToString() => Encoding.ASCII.GetString(this.chars.AsSpan());
}