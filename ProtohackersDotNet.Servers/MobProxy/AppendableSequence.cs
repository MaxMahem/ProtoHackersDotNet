namespace ProtoHackersDotNet.Servers.MobProxy;

/// <summary>A user creatable and appendable class for creating and appending <see cref="ReadOnlySequence{T}"/>s.
/// Implicitly convertible to <see cref="ReadOnlySequence{T}"/></summary>
/// <remarks>All chains should start with a blank sequence and appended to sequentially.</remarks>
/// <typeparam name="T">The type of the elements in this sequence.</typeparam>
public sealed class AppendableSequence<T>
{
    ReadOnlyMemorySegment<T>? first;
    ReadOnlyMemorySegment<T>? current;

    /// <summary>Appends the values of <paramref name="sequence"/> to the end of the current sequence.</summary>
    /// <param name="sequence">The sequence to append.</param>
    /// <returns>This sequence for chaining.</returns>
    public AppendableSequence<T> Append(ReadOnlySequence<T> sequence)
    {
        SequencePosition position = sequence.Start;
        while (sequence.TryGet(ref position, out ReadOnlyMemory<T> memory, true)) Append(memory);
        return this;
    }

    /// <summary>Appends the value of <paramref name="memory"/> to the current sequence.</summary>
    /// <param name="memory">The memory to append.</param>
    /// <returns>This sequence for chaining.</returns>
    public AppendableSequence<T> Append(ReadOnlyMemory<T> memory)
    {
        if (current is null) first = current = new ReadOnlyMemorySegment<T>(memory);
        else current = current.Append(memory);
        return this;
    }

    /// <summary>Returns this as a <see cref="ReadOnlySequence{T}"/>.</summary>
    /// <returns>The current data, encoded as a <see cref="ReadOnlySequence{T}"/>.</returns>
    public ReadOnlySequence<T> AsSequence() => first is not null && current is not null
            ? new ReadOnlySequence<T>(first, 0, current, current.Memory.Length)
            : ReadOnlySequence<T>.Empty;

    /// <summary>Converts this object into a <see cref="ReadOnlySequence{T}"/>.</summary>
    /// <param name="sequence">The sequence to convert.</param>
    public static implicit operator ReadOnlySequence<T>(AppendableSequence<T> sequence) => sequence.AsSequence();

    /// <summary>Internal memory segment class.</summary>
    /// <typeparam name="TMemory">The type of the segment, should match <typeparamref name="T"/></typeparam>
    sealed class ReadOnlyMemorySegment<TMemory> : ReadOnlySequenceSegment<TMemory>
    {
        /// <summary>Creates a new <see cref="ReadOnlyMemorySegment{TMemory}"/> with an initial value of 
        /// <paramref name="memory"/>.</summary>
        /// <param name="memory">The <see cref="ReadOnlyMemory{T}"/> to initialize the segment with.</param>
        public ReadOnlyMemorySegment(ReadOnlyMemory<TMemory> memory) => Memory = memory;

        /// <summary>Appends <paramref name="memory"/> to the next operator of this memory as a new 
        /// <see cref="ReadOnlyMemorySegment{TMemory}"/>, chaining them.</summary>
        /// <param name="memory">The memory to append.</param>
        /// <returns>The new <see cref="ReadOnlyMemorySegment{TMemory}"/> that was chained to this object.</returns>
        public ReadOnlyMemorySegment<TMemory> Append(ReadOnlyMemory<TMemory> memory)
        {
            ReadOnlyMemorySegment<TMemory> nextChunk = new(memory) { RunningIndex = RunningIndex + Memory.Length };
            Next = nextChunk;
            return nextChunk;
        }
    }
}