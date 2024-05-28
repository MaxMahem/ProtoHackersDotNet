namespace ProtoHackersDotNet.AsciiString;

public ref struct ValueAsciiBuilder
{
    byte[]? rentedPoolArray;
    Span<byte> asciiChars = [];
    int position = 0;

    public ValueAsciiBuilder(Span<byte> initialBuffer)
    {
        this.rentedPoolArray = null;
        this.asciiChars = initialBuffer;
    }

    public ValueAsciiBuilder(int initialCapacity)
    {
        this.rentedPoolArray = ArrayPool<byte>.Shared.Rent(initialCapacity);
        this.asciiChars = this.rentedPoolArray;
    }

    public int Length {
        readonly get => position;
        set {
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(value, this.asciiChars.Length);
            this.position = value;
        }
    }

    public readonly int Capacity => asciiChars.Length;

    public void EnsureCapacity(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);

        if (capacity > this.asciiChars.Length)
            Grow(capacity - this.position);
    }

    private void EnsureAdditionalCapacity(int additionalCapacity) => EnsureCapacity(this.position + additionalCapacity);

    public ref byte this[int index] {
        get {
            ArgumentOutOfRangeException.ThrowIfGreaterThan(index, this.position);
            return ref this.asciiChars[index];
        }
    }

    public override readonly string ToString() => Encoding.ASCII.GetString(this.asciiChars[..this.position]);

    public readonly ascii ToAscii() => new(this.asciiChars[..this.position]);

    public ReadOnlySpan<byte> AsSpan(bool terminate)
    {
        if (terminate) {
            EnsureAdditionalCapacity(1);
            this.asciiChars[Length] = (byte) '\0';
        }
        return this.asciiChars[..this.position];
    }

    public readonly ReadOnlySpan<byte> AsSpan() => this.asciiChars[..this.position];
    public readonly ReadOnlySpan<byte> AsSpan(int start) => this.asciiChars[start..];
    public readonly ReadOnlySpan<byte> AsSpan(int start, int length) => this.asciiChars[start..length];

    public void Insert(int index, byte value, int count)
    {
        EnsureAdditionalCapacity(count);

        this.asciiChars[index..this.position].CopyTo(this.asciiChars[(index + count)..]);
        this.asciiChars.Slice(this.position, count).Fill(value);
        this.position += count;
    }

    public void Insert(int index, ascii insertString)
    {
        EnsureAdditionalCapacity(insertString.Length);
        Guard.IsLessThan(index, this.position);

        this.asciiChars[index..position].CopyTo(asciiChars[(index + insertString.Length)..]);
        insertString.CopyTo(this.asciiChars[index..]);
        this.position += insertString.Length;
    }

    public void Append(ascii asciiString)
    {
        EnsureAdditionalCapacity(asciiString.Length);

        asciiString.CopyTo(this.asciiChars[this.position..]);
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

        this.asciiChars.Slice(this.position, count).Fill(c);
        this.position += count;
    }

    public void Append(char c, int count = 1)
    {
        if (!char.IsAscii(c)) ThrowHelper.ThrowArgumentException($"'{c}' is not a valid ASCII character");
        EnsureAdditionalCapacity(count);

        this.asciiChars.Slice(this.position, count).Fill((byte) c);
        this.position += count;
    }

    public void Append(scoped ReadOnlySpan<byte> span)
    {
        EnsureAdditionalCapacity(span.Length);

        span.CopyTo(this.asciiChars[this.position..]);
        this.position += span.Length;
    }

    public void AppendJoin(ascii delimiter, IEnumerable<ascii> asciis)
    {
        using var enumerator = asciis.GetEnumerator();
        if (!enumerator.MoveNext()) return;

        Append(enumerator.Current);

        while (enumerator.MoveNext()) {
            Append(delimiter);
            Append(enumerator.Current);
        }
    }

    private void Grow(int additionalCapacityBeyondPos)
    {
        // Minimum capacity increase is twice the current size, up to Array.MaxLength. User can request more though.
        int minimumCapacityIncrease = int.Min(this.asciiChars.Length * 2, Array.MaxLength);
        int newCapacity = int.Max(this.position + additionalCapacityBeyondPos, minimumCapacityIncrease);

        // any grown array will be rented from ArrayPool
        byte[] newRentedPool = ArrayPool<byte>.Shared.Rent(newCapacity);
        this.asciiChars[..this.position].CopyTo(newRentedPool);

        // original array might be stackallocated and not from the array pool, otherwise, return the old pool
        if (this.rentedPoolArray is not null) ArrayPool<byte>.Shared.Return(this.rentedPoolArray);

        this.asciiChars = this.rentedPoolArray = newRentedPool;
    }

    public readonly void Dispose()
    {
        if (this.rentedPoolArray is not null) ArrayPool<byte>.Shared.Return(rentedPoolArray);
    }

}
