using System.Text;

namespace ProtoHackersDotNet.AsciiString;

public ref struct ValueAsciiBuilder
{
    byte[]? rentedPoolArray;
    Span<byte> asciiChars = [];
    int position = 0;

    public ValueAsciiBuilder(Span<byte> initialBuffer)
    {
        rentedPoolArray = null;
        asciiChars = initialBuffer;
    }

    public ValueAsciiBuilder(int initialCapacity)
    {
        rentedPoolArray = ArrayPool<byte>.Shared.Rent(initialCapacity);
        asciiChars = rentedPoolArray;
    }

    public int Length {
        readonly get => position;
        set {
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(value, asciiChars.Length);
            position = value;
        }
    }

    public readonly int Capacity => asciiChars.Length;

    public void EnsureCapacity(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);

        if (capacity > asciiChars.Length)
            Grow(capacity - position);
    }

    private void EnsureAdditionalCapacity(int additionalCapacity) => EnsureCapacity(Capacity + additionalCapacity);

    public ref byte this[int index] {
        get {
            ArgumentOutOfRangeException.ThrowIfGreaterThan(index, position);
            return ref asciiChars[index];
        }
    }

    public override readonly string ToString() => Encoding.ASCII.GetString(asciiChars[..position]);

    public readonly Ascii ToAscii() => new(asciiChars[..position]);

    public ReadOnlySpan<byte> AsSpan(bool terminate)
    {
        if (terminate) {
            EnsureAdditionalCapacity(1);
            asciiChars[Length] = (byte) '\0';
        }
        return asciiChars[..position];
    }

    public readonly ReadOnlySpan<byte> AsSpan() => asciiChars[..position];
    public readonly ReadOnlySpan<byte> AsSpan(int start) => asciiChars[start..position];
    public readonly ReadOnlySpan<byte> AsSpan(int start, int length) => asciiChars.Slice(start, length);

    public void Insert(int index, byte value, int count)
    {
        EnsureAdditionalCapacity(count);

        asciiChars[index..position].CopyTo(asciiChars[(index + count)..]);
        asciiChars.Slice(index, count).Fill(value);
        position += count;
    }

    public void Insert(int index, Ascii insertString)
    {
        EnsureAdditionalCapacity(insertString.Length);
        Guard.IsLessThan(index, this.position);

        this.asciiChars[index..position].CopyTo(asciiChars[(index + insertString.Length)..]);
        insertString.CopyTo(this.asciiChars[index..]);
        this.position += insertString.Length;
    }

    public void Append(Ascii asciiString)
    {
        EnsureAdditionalCapacity(asciiString.Length);

        asciiString.CopyTo(asciiChars[this.position..]);
        this.position += asciiString.Length;
    }

    public void Append(ReadOnlySequence<byte> sequence)
    {
        EnsureAdditionalCapacity((int) sequence.Length);

        SequencePosition seqPosition = sequence.Start;
        while (sequence.TryGet(ref seqPosition, out var memory))
            memory.Span.CopyTo(this.asciiChars[this.position..]);
        this.position += (int) sequence.Length;
    }

    public void Append(byte c, int count = 1)
    {
        EnsureAdditionalCapacity(count);

        asciiChars.Slice(position, count).Fill(c);
        position += count;
    }

    public void Append(scoped ReadOnlySpan<byte> span)
    {
        EnsureAdditionalCapacity(span.Length);

        span.CopyTo(asciiChars[position..]);
        position += span.Length;
    }

    public void AppendJoin(Ascii delimiter, IEnumerable<Ascii> asciis)
    {
        using var enumerator = asciis.GetEnumerator();
        if (!enumerator.MoveNext())
            return;

        Append(enumerator.Current);

        while (enumerator.MoveNext()) {
            Append(delimiter);
            Append(enumerator.Current);
        }
    }

    private void Grow(int additionalCapacityBeyondPos)
    {
        // Minimum capacity increase is twice the current size, up to Array.MaxLength. User can request more though.
        int minimumCapacityIncrease = int.Min(asciiChars.Length * 2, Array.MaxLength);
        int newCapacity = int.Max(position + additionalCapacityBeyondPos, minimumCapacityIncrease);

        byte[] newRentedPool = ArrayPool<byte>.Shared.Rent(newCapacity);
        asciiChars[..position].CopyTo(newRentedPool);

        byte[]? toReturn = rentedPoolArray;
        if (toReturn != null)
            ArrayPool<byte>.Shared.Return(toReturn);

        asciiChars = rentedPoolArray = newRentedPool;
    }

    public readonly void Dispose()
    {
        if (rentedPoolArray is not null)
            ArrayPool<byte>.Shared.Return(rentedPoolArray);
    }

}
