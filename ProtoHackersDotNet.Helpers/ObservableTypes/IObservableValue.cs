
namespace ProtoHackersDotNet.Helpers.ObservableTypes;

/// <summary>Represents an observable value that notifies subscribers of changes.</summary>
/// <typeparam name="T">The type of the value.</typeparam>
public interface IObservableValue<T> : IReadOnlyObservableValue<T>
{
    /// <summary>Gets or sets the latest value.</summary>
    /// <remarks>Setting this property will trigger a notification to subscribers of <see cref="Changes"/>.</remarks>
    new T Value { get; set; }

    void Complete();
    void Error(Exception exception);
}