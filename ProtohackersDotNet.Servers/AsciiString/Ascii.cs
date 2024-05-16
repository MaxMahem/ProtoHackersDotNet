using CommunityToolkit.Diagnostics;
using System.IO.Hashing;
using System.Text;

namespace ProtoHackersDotNet.AsciiString;

public readonly partial struct Ascii : IEquatable<Ascii>, IComparable<Ascii>
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
    Ascii(byte[]? array) => this.chars = array;

    #endregion

    #region Constructors

    public Ascii(Span<byte> span) => this.chars = span.ToArray();

    public Ascii(string str) => this.chars = [.. Encoding.ASCII.GetBytes(str)];

    public Ascii(ReadOnlySequence<byte> sequence) => this.chars = sequence.ToArray();

    #endregion

    [SuppressMessage("Performance", "CA1825:Avoid zero-length array allocations", Justification = "Designed copied from ImmutableArray")]
    public static readonly Ascii Empty = new(new byte[0]);

    #region Equality and Hash

    /// <summary>Returns a hash code for this Ascii.</summary>
    /// <returns>A hash code for this instance, equal values should hash equally.</returns>
    public override int GetHashCode()
    {
        Ascii self = this;
        return self.chars is null ? 0 : unchecked((int) XxHash32.HashToUInt32(self.chars));
    }

    /// <summary>Determines whether the specified <see cref="object"/> is equal to this Ascii.</summary>
    /// <param name="obj">The <see cref="object"/> to compare with this Ascii.</param>
    /// <returns><c>true</c> if the specified <see cref="object"/> is equal to this instance; otherwise, <c>false</c>.</returns>
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is Ascii other && other == this;

    /// <summary>Indicates whether the current Ascii is equal to another Ascii.</summary>
    /// <param name="other">An Ascii to compare with.</param>
    /// <returns><c>true</c> if the current Ascii has an equal value to <paramref name="other"/> parameter; otherwise, <c>false</c>.
    /// </returns>
    public bool Equals(Ascii other)
    {
        Ascii self = this;
        
        if (ReferenceEquals(self.chars, other.chars)) return true;
        if (self.Length != other.Length) return false;
        return self.chars.AsSpan().SequenceEqual(other.chars);
    }

    public bool IsEmpty() => this.Length <= 0;

    /// <inheritdoc/>
    /// TODO: ENSURE THIS WORKS!
    public int CompareTo(Ascii other)
    {
        Ascii self = this;

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
    public static bool operator ==(Ascii left, Ascii right) => left.chars.AsSpan().SequenceEqual(right.chars);

    /// <summary>Checks value inequality between two Ascii.</summary>
    /// <param name="left">The Ascii to the left of the operator.</param>
    /// <param name="right">The Ascii to the right of the operator.</param>
    /// <returns><c>true</c> if the two ascii have different values; <c>false</c> otherwise.</returns>
    public static bool operator !=(Ascii left, Ascii right) => !left.chars.AsSpan().SequenceEqual(right.chars);

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
        Ascii self = this;
        self.ThrowNullRefIfNotInitialized();
        Array.Copy(self.chars!, destination, self.Length);
    }

    /// <summary>Copies the contents of this Ascii to the specified array.</summary>
    /// <param name="destination">The array to copy to.</param>
    /// <param name="destinationIndex">The index into the destination array to which the first copied char is written.</param>
    public void CopyTo(byte[] destination, int destinationIndex)
    {
        Ascii self = this;
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
        Ascii self = this;
        self.ThrowNullRefIfNotInitialized();
        Array.Copy(self.chars!, sourceIndex, destination, destinationIndex, length);
    }

    /// <summary>Copies the contents of this Ascii to <paramref name="destination"/>.</summary>
    /// <param name="destination">The <see cref="Span{byte}"/> that is the destination of the characters copied.</param>
    public void CopyTo(Span<byte> destination)
    {
        Ascii self = this;
        self.ThrowNullRefIfNotInitialized();
        Guard.HasSizeGreaterThan(destination, self.Length);
        self.AsSpan().CopyTo(destination);
    }

    #endregion

    public bool Contains(Ascii needle) => chars.AsSpan().IndexOf(needle.AsSpan()) >= 0;

    public static Ascii operator +(Ascii a, Ascii b)
    {
        int length = a.Length + b.Length;
        var buffer = length <= 1024 ? stackalloc byte[length] : new byte[length];

        a.chars.CopyTo(buffer);
        b.chars.CopyTo(buffer[a.Length..]);
        return new(buffer);
    }

    public static Ascii operator+(Ascii a, byte b)
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