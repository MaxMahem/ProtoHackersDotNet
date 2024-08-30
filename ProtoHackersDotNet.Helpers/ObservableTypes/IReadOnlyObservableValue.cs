
namespace ProtoHackersDotNet.Helpers.ObservableTypes;

/// <summary>Represents a readonly observable value that notifies subscribers of changes.</summary>
/// <typeparam name="T">The type of the value.</typeparam>
public interface IReadOnlyObservableValue<T> : IDisposable
{
    /// <summary>Gets an observable sequence that produces values whenever the <see cref="Value"/> property changes.</summary>
    IObservable<T> Changes { get; }

    /// <summary>Gets the latest value.</summary>
    T Value { get; }
}