using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace ProtoHackersDotNet.Helpers.ObservableTypes;

/// <summary>Wraps a value in an observable that reports changes to its status.</summary>
/// <typeparam name="T">The type of the value.</typeparam>
/// <param name="initialValue">The value to initialize this object with.</param>
public sealed class ObservableValue<T>(T initialValue) : IDisposable, IObservableValue<T>
{
    readonly BehaviorSubject<T> valueObserver = new(initialValue);

    public T Value {
        get => this.valueObserver.Value;
        set => this.valueObserver.OnNext(value);
    }

    public IObservable<T> Changes => this.valueObserver.AsObservable();

    /// <summary>Notifies all observers of completion of the sequence.</summary>
    /// <remarks>Note, after doing this the value can no longer be changed!</remarks>
    public void Complete() => this.valueObserver.OnCompleted();

    /// <summary>Disposes of this value, unsubscribing all observers. 
    /// After calling this method this value can no longer be read!</summary>
    public void Dispose() => this.valueObserver.Dispose();
}
